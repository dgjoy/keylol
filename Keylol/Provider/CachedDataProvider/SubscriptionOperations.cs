using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Provider.CachedDataProvider
{
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

        private async Task<string> InitUserSubscribedTargetsAsync([NotNull] string userId)
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
            await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
            return cacheKey;
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
            if (userId == null || targetId == null)
                return false;
            var cacheKey = await InitUserSubscribedTargetsAsync(userId);
            return await _redis.GetDatabase()
                .SetContainsAsync(cacheKey, UserSubscribedTargetCacheValue(targetId, targetType));
        }

        /// <summary>
        /// 获取指定用户的订阅数量（关注数量）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>用户的订阅数量</returns>
        public async Task<long> GetSubscriptionCountAsync([NotNull] string userId)
        {
            var cacheKey = await InitUserSubscribedTargetsAsync(userId);
            return await _redis.GetDatabase().SetLengthAsync(cacheKey);
        }

        /// <summary>
        /// 获取指定用户的好友（相互关注）数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>用户的好友数量</returns>
        public async Task<long> GetFriendCountAsync([NotNull] string userId)
        {
            var cacheKey = await InitUserSubscribedTargetsAsync(userId);
            long count = 0;
            foreach (var member in await _redis.GetDatabase().SetMembersAsync(cacheKey))
            {
                var parts = ((string) member).Split(':');
                var type = parts[0].ToCase(NameConventionCase.DashedCase, NameConventionCase.PascalCase)
                    .ToEnum<SubscriptionTargetType>();
                if (type == SubscriptionTargetType.User &&
                    await IsSubscribedAsync(parts[1], userId, SubscriptionTargetType.User))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取指定目标的订阅者数量
        /// </summary>
        /// <param name="targetId">目标 ID</param>
        /// <param name="targetType">目标类型</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetId"/> 为 null</exception>
        /// <returns>目标的订阅者数量</returns>
        public async Task<long> GetSubscriberCountAsync([NotNull] string targetId, SubscriptionTargetType targetType)
        {
            if (targetId == null)
                throw new ArgumentNullException(nameof(targetId));
            var cacheKey = TargetSubscriberCountCacheKey(targetId, targetType);
            var redisDb = _redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
            {
                await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
                return (long) cacheResult;
            }

            var subscriberCount = await _dbContext.Subscriptions
                .LongCountAsync(s => s.TargetId == targetId && s.TargetType == targetType);
            await redisDb.StringSetAsync(cacheKey, subscriberCount, CachedDataProvider.DefaultTtl);
            return subscriberCount;
        }

        private async Task IncreaseSubscriberCountAsync([NotNull] string targetId, SubscriptionTargetType targetType,
            long value)
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
        /// <exception cref="ArgumentNullException">有参数为 null</exception>
        public async Task AddAsync([NotNull] string subscriberId, [NotNull] string targetId,
            SubscriptionTargetType targetType)
        {
            if (subscriberId == null)
                throw new ArgumentNullException(nameof(subscriberId));
            if (targetId == null)
                throw new ArgumentNullException(nameof(targetId));

            if (subscriberId == targetId || await IsSubscribedAsync(subscriberId, targetId, targetType))
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
        /// <exception cref="ArgumentNullException">有参数为 null</exception>
        public async Task RemoveAsync([NotNull] string subscriberId, [NotNull] string targetId,
            SubscriptionTargetType targetType)
        {
            if (subscriberId == null)
                throw new ArgumentNullException(nameof(subscriberId));
            if (targetId == null)
                throw new ArgumentNullException(nameof(targetId));

            if (subscriberId == targetId || !await IsSubscribedAsync(subscriberId, targetId, targetType))
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
}