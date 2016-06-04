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

                        var count = 0;
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
                                if (await AddOrUpdateFeedAsync(UserStream.Name(article.AuthorId),
                                    article.Id, FeedEntryType.ArticleId, null, dbContext))
                                    count++;

                                foreach (var subscriberId in await dbContext.Subscriptions
                                    .Where(s => s.TargetId == article.AuthorId &&
                                                s.TargetType == SubscriptionTargetType.User)
                                    .Select(s => s.SubscriberId).ToListAsync())
                                {
                                    if (await AddOrUpdateFeedAsync(SubscriptionStream.Name(subscriberId),
                                        article.Id, FeedEntryType.ArticleId, "author", dbContext))
                                        count++;
                                }

                                var pointIds = Helpers.SafeDeserialize<List<string>>(article.AttachedPoints) ??
                                               new List<string>();
                                pointIds.Add(article.TargetPointId);
                                foreach (var pointId in pointIds)
                                {
                                    var point = await dbContext.Points.Where(p => p.Id == pointId)
                                        .Select(p => new {p.Id}).SingleOrDefaultAsync();
                                    if (point == null) continue;
                                    if (await AddOrUpdateFeedAsync(PointStream.Name(point.Id),
                                        article.Id, FeedEntryType.ArticleId, null, dbContext))
                                        count++;

                                    foreach (var subscriberId in await dbContext.Subscriptions
                                        .Where(s => s.TargetId == point.Id &&
                                                    s.TargetType == SubscriptionTargetType.Point)
                                        .Select(s => s.SubscriberId).ToListAsync())
                                    {
                                        if (await AddOrUpdateFeedAsync(SubscriptionStream.Name(subscriberId),
                                            article.Id, FeedEntryType.ArticleId, $"point:{point.Id}", dbContext))
                                            count++;
                                    }
                                }
                                break;
                            }
                            case ContentPushType.Activity:
                            case ContentPushType.ConferenceEntry:
                                throw new NotImplementedException();
                            default:
                                throw new ArgumentOutOfRangeException();
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