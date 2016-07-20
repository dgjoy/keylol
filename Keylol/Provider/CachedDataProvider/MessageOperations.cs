using System;
using System.Data.Entity;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Hubs;
using Keylol.Models;
using Keylol.Models.DAL;

namespace Keylol.Provider.CachedDataProvider
{
    /// <summary>
    /// 负责邮政消息相关操作
    /// </summary>
    public class MessageOperations
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;

        /// <summary>
        /// 创建 <see cref="MessageOperations"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public MessageOperations(KeylolDbContext dbContext, RedisProvider redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        private static string UserUnreadMessageCountKey(string userId) => $"user-unread-message-count:{userId}";

        /// <summary>
        ///     获取指定用户未读消息数
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <exception cref="ArgumentNullException"><paramref name="userId"/> 为 null</exception>
        /// <returns>未读消息数</returns>
        public async Task<int> GetUserUnreadMessageCountAsync([NotNull] string userId)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var cacheKey = UserUnreadMessageCountKey(userId);
            var redisDb = _redis.GetDatabase();
            var cachedResult = await redisDb.StringGetAsync(cacheKey);
            if (cachedResult.HasValue)
            {
                await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
                return (int) cachedResult;
            }

            var unreadCount = await _dbContext.Messages.CountAsync(m => m.ReceiverId == userId && m.Unread);
            await redisDb.StringSetAsync(cacheKey, unreadCount, CachedDataProvider.DefaultTtl);
            return unreadCount;
        }

        /// <summary>
        /// 增加指定用户的未读邮政消息数
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="value">变化量</param>
        public async Task IncreaseUserUnreadMessageCountAsync([NotNull] string userId, long value)
        {
            var cacheKey = UserUnreadMessageCountKey(userId);
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
        /// 向数据库添加并一条邮政消息并保存
        /// </summary>
        /// <param name="message">消息对象</param>
        public async Task AddAsync([NotNull] Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            await IncreaseUserUnreadMessageCountAsync(message.ReceiverId, 1);
            NotificationProvider.Hub<MessageHub, IMessageHubClient>().User(message.ReceiverId)?
                .OnUnreadCountChanged(await GetUserUnreadMessageCountAsync(message.ReceiverId));
        }
    }
}