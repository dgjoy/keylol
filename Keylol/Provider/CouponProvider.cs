using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Newtonsoft.Json;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供文券操作服务
    /// </summary>
    public class CouponProvider
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;
        private readonly KeylolUserManager _userManager;

        private static string UnreadLogsCacheKey(string userId) => $"user-unread-coupon-logs:{userId}";

        /// <summary>
        /// 创建新 <see cref="CouponProvider"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public CouponProvider(KeylolDbContext dbContext, RedisProvider redis, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _redis = redis;
            _userManager = userManager;
        }

        /// <summary>
        /// 根据文券事件更新用户的文券数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="event">文券事件</param>
        /// <param name="description">文券记录描述，会被序列化成 JSON 存储到数据库</param>
        /// <param name="logTime">文券日志记录时间，如果为 null 则使用当前时间</param>
        public async Task Update(string userId, CouponEvent @event, object description = null, DateTime? logTime = null)
        {
            await Update(userId, @event, @event.ToCouponChange(), description, logTime);
        }

        /// <summary>
        /// 增减用户的文券数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="event">文券事件</param>
        /// <param name="change">文券数量变化，正数为增加，负数为减少</param>
        /// <param name="description">文券记录描述</param>
        /// <param name="logTime">文券日志记录时间，如果为 null 则使用当前时间</param>
        public async Task Update(string userId, CouponEvent @event, int change, object description = null,
            DateTime? logTime = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("指定用户不存在", nameof(userId));
            var log = new CouponLog
            {
                Change = change,
                Event = @event,
                UserId = userId,
                Description = JsonConvert.SerializeObject(description)
            };
            _dbContext.CouponLogs.Add(log);
            bool saveFailed;
            do
            {
                try
                {
                    saveFailed = false;
                    user.Coupon += log.Change;
                    log.Balance = user.Coupon;
                    log.CreateTime = logTime ?? DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    saveFailed = true;
                    await e.Entries.Single().ReloadAsync();
                }
            } while (saveFailed);
            await _redis.GetDatabase().ListRightPushAsync(UnreadLogsCacheKey(userId),
                RedisProvider.Serialize(new CouponLogDto
                {
                    Change = log.Change,
                    Event = log.Event,
                    Balance = log.Balance
                }));
        }

        /// <summary>
        /// 判断指定用户是否有足够文券触发指定事件
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="event">文券事件</param>
        /// <returns>可以触发指定事件返回 true，不能则返回 false</returns>
        public async Task<bool> CanTriggerEvent(string userId, CouponEvent @event)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user.Coupon + @event.ToCouponChange() >= 0;
        }

        /// <summary>
        /// 取出用户未读的文券变动记录（取出之后将从未读记录中清空）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>未读的 CouponLog 列表</returns>
        public async Task<List<CouponLogDto>> PopUnreadCouponLogs(string userId)
        {
            var redisDb = _redis.GetDatabase();
            var cacheKey = UnreadLogsCacheKey(userId);
            var logs = (await redisDb.ListRangeAsync(cacheKey))
                .Select(v => RedisProvider.Deserialize<CouponLogDto>(v))
                .ToList();
            await redisDb.KeyDeleteAsync(cacheKey);
            return logs;
        }
    }

    /// <summary>
    /// CouponEvent 的一些常用扩展
    /// </summary>
    public static class CouponEventExtensions
    {
        /// <summary>
        /// 获取指定事件的文券变动量
        /// </summary>
        /// <param name="event">文券事件</param>
        /// <returns>变动量，可以为正数或者负数</returns>
        public static int ToCouponChange(this CouponEvent @event)
        {
            switch (@event)
            {
                case CouponEvent.新注册:
                    return 10;

                case CouponEvent.应邀注册:
                    return 5;

                case CouponEvent.发布文章:
                    return -3;

                case CouponEvent.发布简评:
                    return -1;

                case CouponEvent.发出认可:
                    return -1;

                case CouponEvent.获得认可:
                    return 1;

                case CouponEvent.每日访问:
                    return 1;

                case CouponEvent.邀请注册:
                    return 3;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}