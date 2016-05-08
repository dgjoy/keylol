using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
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