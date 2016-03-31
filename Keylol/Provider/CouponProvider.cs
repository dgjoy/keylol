using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Newtonsoft.Json;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供文券操作服务
    /// </summary>
    public class CouponProvider
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建新 <see cref="CouponProvider"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public CouponProvider(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 根据文券事件更新用户的文券数量
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="event">文券事件</param>
        /// <param name="description">文券记录描述，会被序列化成 JSON 存储到数据库</param>
        public async Task Update(string userId, CouponEvent @event, object description = null)
        {
            await Update(userId, @event, @event.CouponChangeAmount(), description);
        }

        /// <summary>
        /// 增减用户的文券数量，文券事件记为 <see cref="CouponEvent.其他"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="change">文券数量变化，正数为增加，负数为减少</param>
        /// <param name="description">文券记录描述</param>
        public async Task Update(string userId, int change, object description = null)
        {
            await Update(userId, CouponEvent.其他, change, description);
        }

        /// <summary>
        /// 判断指定用户是否有足够文券触发指定事件
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="event">文券事件</param>
        /// <returns>可以触发指定事件返回 true，不能则返回 false</returns>
        public bool CanTriggerEvent(string userId, CouponEvent @event)
        {
            var user = _dbContext.Users.Find(userId);
            return user.Coupon + @event.CouponChangeAmount() >= 0;
        }

        private async Task Update(string userId, CouponEvent @event, int change, object description)
        {
            var user = _dbContext.Users.Find(userId);
            var log = _dbContext.CouponLogs.Create();
            log.UserId = user.Id;
            log.Change = change;
            log.Event = @event;
            log.Description = JsonConvert.SerializeObject(description);
            _dbContext.CouponLogs.Add(log);
            bool saveFailed;
            do
            {
                try
                {
                    saveFailed = false;
                    user.Coupon += log.Change;
                    log.Balance = user.Coupon;
                    log.CreateTime = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    saveFailed = true;
                    await e.Entries.Single().ReloadAsync();
                }
            } while (saveFailed);
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
        public static int CouponChangeAmount(this CouponEvent @event)
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