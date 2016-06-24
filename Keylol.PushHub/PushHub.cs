using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Keylol.PushHub
{
    internal sealed class PushHub : KeylolService
    {
        private readonly ILog _logger;
        private readonly IModel _mqChannel;

        public PushHub(ILogProvider logProvider, MqClientProvider mqClientProvider)
        {
            ServiceName = "Keylol.PushHub";

            _logger = logProvider.Logger;
            _mqChannel = mqClientProvider.CreateModel();
        }

        protected override void OnStart(string[] args)
        {
            _mqChannel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(_mqChannel);
            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    using (var streamReader = new StreamReader(new MemoryStream(eventArgs.Body)))
                    using (var dbContext = new KeylolDbContext())
                    {
                        var serializer = new JsonSerializer();
                        var requestDto =
                            serializer.Deserialize<PushHubRequestDto>(new JsonTextReader(streamReader));

                        string entryId;
                        FeedEntryType entryType;
                        List<string> pointsToPush;
                        List<UserToPush> usersToPush = new List<UserToPush>();
                        switch (requestDto.Type)
                        {
                            case ContentPushType.Article:
                            {
                                var article = await dbContext.Articles.Where(a => a.Id == requestDto.ContentId)
                                    .Select(a => new
                                    {
                                        a.Id,
                                        a.AuthorId,
                                        a.AttachedPoints,
                                        a.TargetPointId
                                    }).SingleAsync();
                                usersToPush.Add(new UserToPush
                                {
                                    UserId = article.AuthorId,
                                    SubscriberTimelineReason = "author"
                                });
                                entryId = article.Id;
                                entryType = FeedEntryType.ArticleId;
                                pointsToPush = Helpers.SafeDeserialize<List<string>>(article.AttachedPoints) ??
                                               new List<string>();
                                pointsToPush.Add(article.TargetPointId);
                                break;
                            }

                            case ContentPushType.Activity:
                            {
                                var activity = await dbContext.Activities.Where(a => a.Id == requestDto.ContentId)
                                    .Select(a => new
                                    {
                                        a.Id,
                                        a.AuthorId,
                                        a.AttachedPoints,
                                        a.TargetPointId
                                    }).SingleAsync();
                                usersToPush.Add(new UserToPush
                                {
                                    UserId = activity.AuthorId,
                                    SubscriberTimelineReason = "author"
                                });
                                entryId = activity.Id;
                                entryType = FeedEntryType.ActivityId;
                                pointsToPush = Helpers.SafeDeserialize<List<string>>(activity.AttachedPoints) ??
                                               new List<string>();
                                pointsToPush.Add(activity.TargetPointId);
                                break;
                            }

                            case ContentPushType.Like:
                            {
                                var like = await dbContext.Likes.FindAsync(requestDto.ContentId);
                                entryId = like.TargetId;
                                pointsToPush = new List<string>();
                                usersToPush.Add(new UserToPush
                                {
                                    UserId = like.OperatorId,
                                    UserTimelineReason = "like",
                                    SubscriberTimelineReason = $"like:{like.OperatorId}"
                                });
                                switch (like.TargetType)
                                {
                                    case LikeTargetType.Article:
                                        entryType = FeedEntryType.ArticleId;
                                        break;
                                    case LikeTargetType.Activity:
                                        entryType = FeedEntryType.ActivityId;
                                        break;
                                    default:
                                        _mqChannel.BasicAck(eventArgs.DeliveryTag, false);
                                        return;
                                }
                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var count = 0;
                        foreach (var user in usersToPush)
                        {
                            if (await AddOrUpdateFeedAsync(UserStream.Name(user.UserId),
                                entryId, entryType, user.UserTimelineReason, dbContext))
                                count++;

                            foreach (var subscriberId in await dbContext.Subscriptions
                                .Where(s => s.TargetId == user.UserId &&
                                            s.TargetType == SubscriptionTargetType.User)
                                .Select(s => s.SubscriberId).ToListAsync())
                            {
                                if (await AddOrUpdateFeedAsync(SubscriptionStream.Name(subscriberId),
                                    entryId, entryType, user.SubscriberTimelineReason, dbContext))
                                    count++;
                            }
                        }


                        foreach (var pointId in pointsToPush)
                        {
                            var point = await dbContext.Points.Where(p => p.Id == pointId)
                                .Select(p => new {p.Id}).SingleOrDefaultAsync();
                            if (point == null) continue;
                            if (await AddOrUpdateFeedAsync(PointStream.Name(point.Id),
                                entryId, entryType, null, dbContext))
                                count++;

                            foreach (var subscriberId in await dbContext.Subscriptions
                                .Where(s => s.TargetId == point.Id &&
                                            s.TargetType == SubscriptionTargetType.Point)
                                .Select(s => s.SubscriberId).ToListAsync())
                            {
                                if (await AddOrUpdateFeedAsync(SubscriptionStream.Name(subscriberId),
                                    entryId, entryType, $"point:{point.Id}", dbContext))
                                    count++;
                            }
                        }

                        _mqChannel.BasicAck(eventArgs.DeliveryTag, false);
                        _logger.Info($"{count} feeds pushed. Content: ({requestDto.Type}) {requestDto.ContentId}");
                    }
                }
                catch (Exception e)
                {
                    _mqChannel.BasicNack(eventArgs.DeliveryTag, false, false);
                    _logger.Fatal("RabbitMQ unhandled callback exception.", e);
                }
            };
            _mqChannel.BasicConsume(MqClientProvider.PushHubRequestQueue, false, consumer);
        }

        protected override void OnStop()
        {
            _mqChannel.Close();
            base.OnStop();
        }

        private static async Task<bool> AddOrUpdateFeedAsync(string streamName, string entry, FeedEntryType entryType,
            string reason, KeylolDbContext dbContext)
        {
            var added = false;
            var startTime = DateTime.Now - TimeSpan.FromDays(1);
            var feed = await dbContext.Feeds.Where(
                f => f.StreamName == streamName && f.Entry == entry &&
                     f.EntryType == entryType &&
                     f.Time > startTime)
                .FirstOrDefaultAsync();
            if (feed == null)
            {
                feed = new Feed
                {
                    StreamName = streamName,
                    EntryType = entryType,
                    Entry = entry
                };
                dbContext.Feeds.Add(feed);
                added = true;
            }
            if (!string.IsNullOrWhiteSpace(reason))
            {
                var properties =
                    Helpers.SafeDeserialize<SubscriptionStream.FeedProperties>(feed.Properties) ??
                    new SubscriptionStream.FeedProperties();
                properties.Reasons = properties.Reasons ?? new List<string>();
                bool reasonExisted;
                string likeOperatorId = null;
                var reasonParts = reason.Split(':');
                if (reasonParts.Length == 2 && reasonParts[0] == "like") likeOperatorId = reasonParts[1];
                if (likeOperatorId == null)
                {
                    reasonExisted = properties.Reasons.Contains(reason);
                }
                else
                {
                    if (properties.Reasons.Count > 0)
                    {
                        var indexToRemove = -1;
                        for (var i = 0; i < properties.Reasons.Count; i++)
                        {
                            var currentReason = properties.Reasons[i];
                            var parts = currentReason.Split(':');
                            if (parts.Length != 2 || parts[0] != "like") continue;
                            var currentSubscriberCount = await dbContext.Subscriptions.LongCountAsync(
                                s => s.TargetId == parts[1] && s.TargetType == SubscriptionTargetType.User);
                            var newSubscriberCount = await dbContext.Subscriptions.LongCountAsync(
                                s => s.TargetId == likeOperatorId && s.TargetType == SubscriptionTargetType.User);
                            if (newSubscriberCount >= currentSubscriberCount)
                                indexToRemove = i;
                            break;
                        }
                        if (indexToRemove >= 0)
                        {
                            properties.Reasons.RemoveAt(indexToRemove);
                            reasonExisted = false;
                        }
                        else
                        {
                            reasonExisted = true;
                        }
                    }
                    else
                    {
                        reasonExisted = false;
                    }
                }
                if (!reasonExisted)
                {
                    properties.Reasons.Add(reason);
                    feed.Properties = JsonConvert.SerializeObject(properties);
                }
            }
            await dbContext.SaveChangesAsync();
            return added;
        }

        private class UserToPush
        {
            public string UserId { get; set; }

            public string UserTimelineReason { get; set; }

            public string SubscriberTimelineReason { get; set; }
        }
    }
}