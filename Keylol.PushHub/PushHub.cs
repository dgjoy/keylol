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

                        string authorId, entryId;
                        FeedEntryType entryType;
                        List<string> pointsToPush;
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
                                    }).SingleOrDefaultAsync();

                                authorId = article.AuthorId;
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
                                    }).SingleOrDefaultAsync();
                                authorId = activity.AuthorId;
                                entryId = activity.Id;
                                entryType = FeedEntryType.ActivityId;
                                pointsToPush = Helpers.SafeDeserialize<List<string>>(activity.AttachedPoints) ??
                                               new List<string>();
                                pointsToPush.Add(activity.TargetPointId);
                                break;
                            }

                            case ContentPushType.ConferenceEntry:
                                throw new NotImplementedException();

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var count = 0;
                        if (await AddOrUpdateFeedAsync(UserStream.Name(authorId),
                            entryId, entryType, null, dbContext))
                            count++;

                        foreach (var subscriberId in await dbContext.Subscriptions
                            .Where(s => s.TargetId == authorId &&
                                        s.TargetType == SubscriptionTargetType.User)
                            .Select(s => s.SubscriberId).ToListAsync())
                        {
                            if (await AddOrUpdateFeedAsync(SubscriptionStream.Name(subscriberId),
                                entryId, entryType, "author", dbContext))
                                count++;
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
                if (!properties.Reasons.Contains(reason))
                {
                    properties.Reasons.Add(reason);
                    feed.Properties = JsonConvert.SerializeObject(properties);
                }
            }
            await dbContext.SaveChangesAsync();
            return added;
        }
    }
}