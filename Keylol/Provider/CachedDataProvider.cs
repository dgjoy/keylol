using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Utilities;

namespace Keylol.Provider
{
    /// <summary>
    ///     提供带有缓存的数据服务
    /// </summary>
    public class CachedDataProvider
    {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(7);

        /// <summary>
        ///     创建新 <see cref="CachedDataProvider" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="redis">
        ///     <see cref="RedisProvider" />
        /// </param>
        public CachedDataProvider(KeylolDbContext dbContext, RedisProvider redis)
        {
            Likes = new LikeOperations(dbContext, redis);
            Subscriptions = new SubscriptionOperations(dbContext, redis);
            Points = new PointOperations(dbContext, redis);
        }

        /// <summary>
        /// 认可
        /// </summary>
        public LikeOperations Likes { get; }

        /// <summary>
        /// 订阅
        /// </summary>
        public SubscriptionOperations Subscriptions { get; }

        /// <summary>
        /// 据点
        /// </summary>
        public PointOperations Points { get; set; }

        /// <summary>
        /// 负责认可相关操作
        /// </summary>
        public class LikeOperations
        {
            private readonly KeylolDbContext _dbContext;
            private readonly RedisProvider _redis;

            /// <summary>
            /// 创建 <see cref="LikeOperations"/>
            /// </summary>
            /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
            /// <param name="redis"><see cref="RedisProvider"/></param>
            public LikeOperations(KeylolDbContext dbContext, RedisProvider redis)
            {
                _dbContext = dbContext;
                _redis = redis;
            }

            private static string UserLikeCountCacheKey(string userId) => $"user-like-count:{userId}";

            private static string TargetLikeCountCacheKey(string targetId, LikeTargetType targetType) =>
                $"target-like-count:{targetType.ToString().ToCase(NameConventionCase.PascalCase, NameConventionCase.DashedCase)}:{targetId}";

            private static string UserLikedTargetsCacheKey(string userId) => $"user-liked-targets:{userId}";

            private static string UserLikedTargetCacheValue(string targetId, LikeTargetType targetType) =>
                $"{targetType.ToString().ToCase(NameConventionCase.PascalCase, NameConventionCase.DashedCase)}:{targetId}";

            /// <summary>
            ///     获取指定用户获得的总认可数
            /// </summary>
            /// <param name="userId">用户 ID</param>
            /// <returns>用户获得的总认可数</returns>
            public async Task<int> GetUserLikeCountAsync(string userId)
            {
                var cacheKey = UserLikeCountCacheKey(userId);
                var redisDb = _redis.GetDatabase();
                var cachedResult = await redisDb.StringGetAsync(cacheKey);
                if (cachedResult.HasValue)
                    return (int) cachedResult;

                var articleLikeCount = await (from article in _dbContext.Articles
                    join like in _dbContext.Likes on article.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.Article && article.AuthorId == userId
                    select article
                    ).CountAsync();
                var articleCommentLikeCount = await (from comment in _dbContext.ArticleComments
                    join like in _dbContext.Likes on comment.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.ArticleComment && comment.CommentatorId == userId
                    select comment
                    ).CountAsync();
                var activityLikeCount = await (from activity in _dbContext.Activities
                    join like in _dbContext.Likes on activity.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.Activity && activity.AuthorId == userId
                    select activity
                    ).CountAsync();
                var activityCommentLikeCount = await (from comment in _dbContext.ActivityComments
                    join like in _dbContext.Likes on comment.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.ActivityComment && comment.CommentatorId == userId
                    select comment
                    ).CountAsync();
                var conferenceEntryLikeCount = await (from entry in _dbContext.ConferenceEntries
                    join like in _dbContext.Likes on entry.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.ConferenceEntry && entry.AuthorId == userId
                    select entry
                    ).CountAsync();

                var likeCount = articleLikeCount + articleCommentLikeCount + activityLikeCount +
                                activityCommentLikeCount + conferenceEntryLikeCount;
                await redisDb.StringSetAsync(cacheKey, likeCount, DefaultTtl);
                return likeCount;
            }

            /// <summary>
            /// 获取指定目标获取的认可数
            /// </summary>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            /// <returns>指定目标获取的认可数</returns>
            public async Task<int> GetTargetLikeCountAsync(string targetId, LikeTargetType targetType)
            {
                var cacheKey = TargetLikeCountCacheKey(targetId, targetType);
                var redisDb = _redis.GetDatabase();
                var cachedResult = await redisDb.StringGetAsync(cacheKey);
                if (cachedResult.HasValue)
                    return (int) cachedResult;

                var likeCount =
                    await _dbContext.Likes.CountAsync(l => l.TargetId == targetId && l.TargetType == targetType);
                await redisDb.StringSetAsync(cacheKey, likeCount, DefaultTtl);
                return likeCount;
            }

            /// <summary>
            /// 判断指定用户是否认可过指定目标
            /// </summary>
            /// <param name="userId">用户 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            /// <returns>如果用户认可过，返回 <c>true</c></returns>
            public async Task<bool> IsLikedAsync(string userId, string targetId, LikeTargetType targetType)
            {
                var cacheKey = UserLikedTargetsCacheKey(userId);
                var redisDb = _redis.GetDatabase();
                if (!await redisDb.KeyExistsAsync(cacheKey))
                {
                    foreach (var like in await _dbContext.Likes.Where(l => l.OperatorId == userId)
                        .Select(l => new {l.TargetId, l.TargetType}).ToListAsync())
                    {
                        await redisDb.SetAddAsync(cacheKey, UserLikedTargetCacheValue(like.TargetId, like.TargetType));
                    }
                }
                await redisDb.KeyExpireAsync(cacheKey, DefaultTtl);
                return await redisDb.SetContainsAsync(cacheKey, UserLikedTargetCacheValue(targetId, targetType));
            }

            private async Task IncreaseUserLikeCount(string userId, long value)
            {
                var cacheKey = UserLikeCountCacheKey(userId);
                var redisDb = _redis.GetDatabase();
                if (await redisDb.KeyExistsAsync(cacheKey))
                {
                    if (value >= 0)
                        await redisDb.StringIncrementAsync(cacheKey, value);
                    else
                        await redisDb.StringDecrementAsync(cacheKey, -value);
                }
            }

            private async Task IncreaseTargetLikeCount(string targetId, LikeTargetType targetType, long value)
            {
                var cacheKey = TargetLikeCountCacheKey(targetId, targetType);
                var redisDb = _redis.GetDatabase();
                if (await redisDb.KeyExistsAsync(cacheKey))
                {
                    if (value >= 0)
                        await redisDb.StringIncrementAsync(cacheKey, value);
                    else
                        await redisDb.StringDecrementAsync(cacheKey, -value);
                }
            }

            /// <summary>
            /// 添加一个新认可
            /// </summary>
            /// <param name="operatorId">认可操作者 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            public async Task AddAsync(string operatorId, string targetId, LikeTargetType targetType)
            {
                if (await IsLikedAsync(operatorId, targetId, targetType))
                    return;

                string likeReceiverId;
                switch (targetType)
                {
                    case LikeTargetType.Article:
                        likeReceiverId = await _dbContext.Articles.Where(a => a.Id == targetId)
                            .Select(a => a.AuthorId).SingleAsync();
                        break;

                    case LikeTargetType.ArticleComment:
                        likeReceiverId = await _dbContext.ArticleComments.Where(c => c.Id == targetId)
                            .Select(c => c.CommentatorId).SingleAsync();
                        break;

                    case LikeTargetType.Activity:
                        likeReceiverId = await _dbContext.Activities.Where(a => a.Id == targetId)
                            .Select(a => a.AuthorId).SingleAsync();
                        break;

                    case LikeTargetType.ActivityComment:
                        likeReceiverId = await _dbContext.ActivityComments.Where(c => c.Id == targetId)
                            .Select(c => c.CommentatorId).SingleAsync();
                        break;

                    case LikeTargetType.ConferenceEntry:
                        likeReceiverId = await _dbContext.ConferenceEntries.Where(e => e.Id == targetId)
                            .Select(e => e.AuthorId).SingleAsync();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }

                _dbContext.Likes.Add(new Like
                {
                    OperatorId = operatorId,
                    TargetId = targetId,
                    TargetType = targetType
                });
                await _dbContext.SaveChangesAsync();

                var redisDb = _redis.GetDatabase();
                await redisDb.SetAddAsync(UserLikedTargetsCacheKey(operatorId),
                    UserLikedTargetCacheValue(targetId, targetType));
                await IncreaseUserLikeCount(likeReceiverId, 1);
                await IncreaseTargetLikeCount(targetId, targetType, 1);
            }

            /// <summary>
            /// 撤销一个操作者发出的认可
            /// </summary>
            /// <param name="operatorId">操作者 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            public async Task RemoveAsync(string operatorId, string targetId, LikeTargetType targetType)
            {
                if (!await IsLikedAsync(operatorId, targetId, targetType))
                    return;

                string likeReceiverId;
                switch (targetType)
                {
                    case LikeTargetType.Article:
                        likeReceiverId = await _dbContext.Articles.Where(a => a.Id == targetId)
                            .Select(a => a.AuthorId).SingleAsync();
                        break;

                    case LikeTargetType.ArticleComment:
                        likeReceiverId = await _dbContext.ArticleComments.Where(c => c.Id == targetId)
                            .Select(c => c.CommentatorId).SingleAsync();
                        break;

                    case LikeTargetType.Activity:
                        likeReceiverId = await _dbContext.Activities.Where(a => a.Id == targetId)
                            .Select(a => a.AuthorId).SingleAsync();
                        break;

                    case LikeTargetType.ActivityComment:
                        likeReceiverId = await _dbContext.ActivityComments.Where(c => c.Id == targetId)
                            .Select(c => c.CommentatorId).SingleAsync();
                        break;

                    case LikeTargetType.ConferenceEntry:
                        likeReceiverId = await _dbContext.ConferenceEntries.Where(e => e.Id == targetId)
                            .Select(e => e.AuthorId).SingleAsync();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }

                var likes = await _dbContext.Likes.Where(l => l.OperatorId == operatorId &&
                                                              l.TargetId == targetId && l.TargetType == targetType)
                    .ToListAsync();
                _dbContext.Likes.RemoveRange(likes);
                await _dbContext.SaveChangesAsync();

                var redisDb = _redis.GetDatabase();
                var cacheKey = UserLikedTargetsCacheKey(operatorId);
                foreach (var like in likes)
                {
                    await redisDb.SetRemoveAsync(cacheKey, UserLikedTargetCacheValue(like.TargetId, like.TargetType));
                }
                await IncreaseUserLikeCount(likeReceiverId, -likes.Count);
                await IncreaseTargetLikeCount(targetId, targetType, -likes.Count);
            }
        }

        /// <summary>
        /// 负责订阅相关操作
        /// </summary>
        public class SubscriptionOperations
        {
            private readonly KeylolDbContext _dbContext;
            private readonly RedisProvider _redis;

            private static string UserSubscribedTargetsCacheKey(string userId) => $"user-subscribed-targets:{userId}";

            private static string UserSubscribedTargetCacheValue(string targetId, SubscriptionTargetType targetType) =>
                $"{targetType.ToString().ToCase(NameConventionCase.PascalCase, NameConventionCase.DashedCase)}:{targetId}";

            private static string TargetSubscriberCountCacheKey(string targetId, SubscriptionTargetType targetType) =>
                $"subscriber-count:{targetType.ToString().ToCase(NameConventionCase.PascalCase, NameConventionCase.DashedCase)}:{targetId}";

            /// <summary>
            /// 创建 <see cref="SubscriptionOperations"/>
            /// </summary>
            /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
            /// <param name="redis"><see cref="RedisProvider"/></param>
            public SubscriptionOperations(KeylolDbContext dbContext, RedisProvider redis)
            {
                _dbContext = dbContext;
                _redis = redis;
            }

            /// <summary>
            /// 判断指定用户是否订阅过指定目标
            /// </summary>
            /// <param name="userId">用户 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            /// <returns>如果用户订阅过，返回 <c>true</c></returns>
            public async Task<bool> IsSubscribedAsync(string userId, string targetId, SubscriptionTargetType targetType)
            {
                var cacheKey = UserSubscribedTargetsCacheKey(userId);
                var redisDb = _redis.GetDatabase();
                if (!await redisDb.KeyExistsAsync(cacheKey))
                {
                    foreach (var subscription in await _dbContext.Subscriptions.Where(s => s.SubscriberId == userId)
                        .Select(s => new {s.TargetId, s.TargetType}).ToListAsync())
                    {
                        await redisDb.SetAddAsync(cacheKey,
                            UserSubscribedTargetCacheValue(subscription.TargetId, subscription.TargetType));
                    }
                }
                await redisDb.KeyExpireAsync(cacheKey, DefaultTtl);
                return await redisDb.SetContainsAsync(cacheKey, UserSubscribedTargetCacheValue(targetId, targetType));
            }

            /// <summary>
            /// 获取指定目标的订阅者数量
            /// </summary>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            /// <returns>目标的订阅者数量</returns>
            public async Task<int> GetSubscriberCountAsync(string targetId, SubscriptionTargetType targetType)
            {
                var cacheKey = TargetSubscriberCountCacheKey(targetId, targetType);
                var redisDb = _redis.GetDatabase();
                var cacheResult = await redisDb.StringGetAsync(cacheKey);
                if (cacheResult.HasValue)
                    return (int) cacheResult;

                var subscriberCount = await _dbContext.Subscriptions
                    .CountAsync(s => s.TargetId == targetId && s.TargetType == targetType);
                await redisDb.StringSetAsync(cacheKey, subscriberCount, DefaultTtl);
                return subscriberCount;
            }

            private async Task IncreaseSubscriberCountAsync(string targetId, SubscriptionTargetType targetType, long value)
            {
                var cacheKey = TargetSubscriberCountCacheKey(targetId, targetType);
                var redisDb = _redis.GetDatabase();
                if (await redisDb.KeyExistsAsync(cacheKey))
                {
                    if (value >= 0)
                        await redisDb.StringIncrementAsync(cacheKey, value);
                    else
                        await redisDb.StringDecrementAsync(cacheKey, -value);
                }
            }

            /// <summary>
            /// 添加一个新订阅
            /// </summary>
            /// <param name="subscriberId">订阅者 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            public async Task AddAsync(string subscriberId, string targetId, SubscriptionTargetType targetType)
            {
                if (await IsSubscribedAsync(subscriberId, targetId, targetType))
                    return;

                _dbContext.Subscriptions.Add(new Subscription
                {
                    SubscriberId = subscriberId,
                    TargetId = targetId,
                    TargetType = targetType
                });
                await _dbContext.SaveChangesAsync();

                var redisDb = _redis.GetDatabase();
                await redisDb.SetAddAsync(UserSubscribedTargetsCacheKey(subscriberId),
                    UserSubscribedTargetCacheValue(targetId, targetType));
                await IncreaseSubscriberCountAsync(targetId, targetType, 1);
            }

            /// <summary>
            /// 撤销一个订阅者已有的订阅
            /// </summary>
            /// <param name="subscriberId">订阅者 ID</param>
            /// <param name="targetId">目标 ID</param>
            /// <param name="targetType">目标类型</param>
            public async Task RemoveAsync(string subscriberId, string targetId, SubscriptionTargetType targetType)
            {
                if (!await IsSubscribedAsync(subscriberId, targetId, targetType))
                    return;

                var subscriptions = await _dbContext.Subscriptions.Where(s => s.SubscriberId == subscriberId &&
                                                                              s.TargetId == targetId &&
                                                                              s.TargetType == targetType)
                    .ToListAsync();
                _dbContext.Subscriptions.RemoveRange(subscriptions);
                await _dbContext.SaveChangesAsync();

                var redisDb = _redis.GetDatabase();
                var cacheKey = UserSubscribedTargetsCacheKey(subscriberId);
                foreach (var subscription in subscriptions)
                {
                    await redisDb.SetRemoveAsync(cacheKey,
                        UserSubscribedTargetCacheValue(subscription.TargetId, subscription.TargetType));
                }
                await IncreaseSubscriberCountAsync(targetId, targetType, -subscriptions.Count);
            }
        }

        /// <summary>
        /// 负责据点相关操作
        /// </summary>
        public class PointOperations
        {
            private readonly KeylolDbContext _dbContext;
            private readonly RedisProvider _redis;

            /// <summary>
            /// 创建 <see cref="PointOperations"/>
            /// </summary>
            /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
            /// <param name="redis"><see cref="RedisProvider"/></param>
            public PointOperations(KeylolDbContext dbContext, RedisProvider redis)
            {
                _dbContext = dbContext;
                _redis = redis;
            }

            private static string RatingCacheKey(string pointId) => $"point-rating:{pointId}";

            /// <summary>
            /// 获取指定据点的评分
            /// </summary>
            /// <param name="pointId">据点 ID</param>
            /// <returns></returns>
            public async Task<PointRatingsDto> GetRatingsAsync(string pointId)
            {
                var cacheKey = RatingCacheKey(pointId);
                var redisDb = _redis.GetDatabase();
                var cacheResult = await redisDb.StringGetAsync(cacheKey);
                if (cacheResult.HasValue)
                    return RedisProvider.Deserialize<PointRatingsDto>(cacheResult);

                var rating = new PointRatingsDto
                {
                    OneStarCount = 5,
                    TwoStarCount = 6,
                    ThreeStarCount = 7,
                    FourStarCount = 8,
                    FiveStarCount = 9,
                    TotalScore = 27,
                    TotalCount = 7
                };
//                await redisDb.StringSetAsync(cacheKey, RedisProvider.Serialize(rating), DefaultTtl);
                return rating;
            }
        }
    }
}