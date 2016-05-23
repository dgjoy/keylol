using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Aggregation.Point.Frontpage
{
    /// <summary>
    /// 入坑用户列表
    /// </summary>
    public class AddictedUserList : List<AddictedUser>
    {
        /// <summary>
        /// 获取入坑用户列表
        /// </summary>
        /// <param name="steamAppId">Steam App ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="AddictedUserList"/></returns>
        public static async Task<AddictedUserList> Get(int steamAppId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.CurrentUser().Identity.GetUserId(), steamAppId, page,
                dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="AddictedUserList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="steamAppId">Steam App ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="AddictedUserList"/></returns>
        public static async Task<AddictedUserList> CreateAsync(string currentUserId, int? steamAppId, int page,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from record in dbContext.UserSteamGameRecords
                where record.SteamAppId == steamAppId && record.UserId != currentUserId
                let isFriend =
                    dbContext.Subscriptions.Any(
                        s =>
                            s.SubscriberId == currentUserId && s.TargetId == record.UserId &&
                            s.TargetType == SubscriptionTargetType.User) &&
                    dbContext.Subscriptions.Any(
                        s =>
                            s.SubscriberId == record.UserId && s.TargetId == currentUserId &&
                            s.TargetType == SubscriptionTargetType.User)
                orderby isFriend descending, record.TotalPlayedTime descending
                select new
                {
                    record.User.Id,
                    record.User.HeaderImage,
                    record.User.IdCode,
                    record.User.AvatarImage,
                    record.User.UserName,
                    record.TotalPlayedTime,
                    IsFriend = isFriend
                }).TakePage(page, 8).ToListAsync();

            var result = new AddictedUserList();
            foreach (var u in queryResult)
            {
                result.Add(new AddictedUser
                {
                    Id = u.Id,
                    HeaderImage = u.HeaderImage,
                    IdCode = u.IdCode,
                    AvatarImage = u.AvatarImage,
                    UserName = u.UserName,
                    TotalPlayedTime = u.TotalPlayedTime,
                    IsFriend = string.IsNullOrWhiteSpace(currentUserId) ? (bool?) null : u.IsFriend,
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, u.Id,
                            SubscriptionTargetType.User)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 入坑用户
    /// </summary>
    public class AddictedUser
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 头部图
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 总在档时间
        /// </summary>
        public double TotalPlayedTime { get; set; }

        /// <summary>
        /// 是否是好友
        /// </summary>
        public bool? IsFriend { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}