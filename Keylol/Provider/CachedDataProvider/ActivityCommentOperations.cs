using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models.DAL;

namespace Keylol.Provider.CachedDataProvider
{
    /// <summary>
    /// 负责动态评论相关操作
    /// </summary>
    public class ActivityCommentOperations
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;

        /// <summary>
        /// 创建 <see cref="ActivityCommentOperations"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public ActivityCommentOperations(KeylolDbContext dbContext, RedisProvider redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        private static string ActivityCommentCountCacheKey(string activityId) => $"activity-comment-count:{activityId}";

        /// <summary>
        /// 获取指定动态的评论数
        /// </summary>
        /// <param name="activityId">动态 ID</param>
        /// <exception cref="ArgumentNullException"><paramref name="activityId"/> 为 null</exception>
        /// <returns>动态的评论数</returns>
        public async Task<int> GetActivityCommentCountAsync([NotNull] string activityId)
        {
            if (activityId == null)
                throw new ArgumentNullException(nameof(activityId));
            var cacheKey = ActivityCommentCountCacheKey(activityId);
            var redisDb = _redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
            {
                await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
                return (int) cacheResult;
            }

            var commentCount = await _dbContext.ActivityComments
                .Where(c => c.ActivityId == activityId)
                .CountAsync();
            await redisDb.StringSetAsync(cacheKey, commentCount, CachedDataProvider.DefaultTtl);
            return commentCount;
        }

        /// <summary>
        /// 增加指定动态的评论数
        /// </summary>
        /// <param name="activityId">动态 ID</param>
        /// <param name="value">数量变化</param>
        public async Task IncreaseActivityCommentCountAsync([NotNull] string activityId, long value)
        {
            var cacheKey = ActivityCommentCountCacheKey(activityId);
            var redisDb = _redis.GetDatabase();
            if (await redisDb.KeyExistsAsync(cacheKey))
            {
                if (value >= 0)
                    await redisDb.StringIncrementAsync(cacheKey, value);
                else
                    await redisDb.StringDecrementAsync(cacheKey, -value);
            }
        }
    }
}