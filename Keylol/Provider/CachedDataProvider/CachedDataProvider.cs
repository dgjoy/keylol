using System;
using Keylol.Models.DAL;

namespace Keylol.Provider.CachedDataProvider
{
    /// <summary>
    ///     提供带有缓存的数据服务
    /// </summary>
    public class CachedDataProvider
    {
        /// <summary>
        /// 默认缓存 TTL
        /// </summary>
        public static TimeSpan DefaultTtl { get; } = TimeSpan.FromDays(7);

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
            Users = new UserOperations(dbContext, redis, Subscriptions);
            ArticleComments = new ArticleCommentOperations(dbContext, redis);
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
        /// 用户
        /// </summary>
        public UserOperations Users { get; set; }

        /// <summary>
        /// Feed
        /// </summary>
        public FeedOperations Feeds { get; set; }

        /// <summary>
        /// 文章评论
        /// </summary>
        public ArticleCommentOperations ArticleComments { get; set; }
    }
}