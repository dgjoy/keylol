using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models.DAL;

namespace Keylol.Provider.CachedDataProvider
{
    /// <summary>
    /// 负责文章评论相关操作
    /// </summary>
    public class ArticleCommentOperations
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;

        /// <summary>
        /// 创建 <see cref="ArticleCommentOperations"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public ArticleCommentOperations(KeylolDbContext dbContext, RedisProvider redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        private static string ArticleCommentCountCacheKey(string articleId) => $"article-comment-count:{articleId}";

        /// <summary>
        /// 获取指定文章的评论数
        /// </summary>
        /// <param name="articleId">文章 ID</param>
        /// <exception cref="ArgumentNullException"><paramref name="articleId"/> 为 null</exception>
        /// <returns>文章的评论数</returns>
        public async Task<int> GetArticleCommentCountAsync([NotNull] string articleId)
        {
            if (articleId == null)
                throw new ArgumentNullException(nameof(articleId));
            var cacheKey = ArticleCommentCountCacheKey(articleId);
            var redisDb = _redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
            {
                await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
                return (int) cacheResult;
            }

            var commentCount = await _dbContext.ArticleComments
                .Where(c => c.ArticleId == articleId)
                .CountAsync();
            await redisDb.StringSetAsync(cacheKey, commentCount, CachedDataProvider.DefaultTtl);
            return commentCount;
        }

        /// <summary>
        /// 增加指定文章的评论数
        /// </summary>
        /// <param name="articleId">文章 ID</param>
        /// <param name="value">数量变化</param>
        public async Task IncreaseArticleCommentCountAsync([NotNull] string articleId, long value)
        {
            var cacheKey = ArticleCommentCountCacheKey(articleId);
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