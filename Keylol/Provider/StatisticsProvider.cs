using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Keylol.Models.DAL;

namespace Keylol.Provider
{
    /// <summary>
    ///     提供统计指标服务
    /// </summary>
    public class StatisticsProvider
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;

        /// <summary>
        ///     创建新 <see cref="StatisticsProvider" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="redis">
        ///     <see cref="RedisProvider" />
        /// </param>
        public StatisticsProvider(KeylolDbContext dbContext, RedisProvider redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        private static string LikeCountCacheKey(string userId) => $"user-like-count:{userId}";

        /// <summary>
        ///     获取用户获得的总认可数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>用户获得的总认可数量</returns>
        public async Task<int> GetUserLikeCount(string userId)
        {
            var cacheKey = LikeCountCacheKey(userId);
            var redisDb = _redis.GetDatabase();
            var cachedResult = await redisDb.StringGetAsync(cacheKey);
            if (cachedResult.HasValue)
                return (int) cachedResult;

            var articleLikeCount = await _dbContext.ArticleLikes.CountAsync(l => l.Article.PrincipalId == userId);
            var commentLikeCount = await _dbContext.CommentLikes.CountAsync(l => l.Comment.CommentatorId == userId);
            var likeCount = articleLikeCount + commentLikeCount;
            await redisDb.StringSetAsync(cacheKey, likeCount, TimeSpan.FromDays(30));
            return likeCount;
        }

        private async Task IncreaseUserLikeCount(string userId, long value)
        {
            var cacheKey = LikeCountCacheKey(userId);
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
        ///     自增用户获得的总认可数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public async Task IncreaseUserLikeCount(string userId)
        {
            await IncreaseUserLikeCount(userId, 1);
        }

        /// <summary>
        ///     自减用户获得的总认可数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public async Task DecreaseUserLikeCount(string userId)
        {
            await IncreaseUserLikeCount(userId, -1);
        }
    }
}