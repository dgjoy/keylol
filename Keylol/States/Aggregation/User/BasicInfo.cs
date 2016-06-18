using System;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.User
{
    /// <summary>
    /// 用户基础信息
    /// </summary>
    public class BasicInfo
    {
        /// <summary>
        /// 创建 <see cref="BasicInfo"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="user">用户对象</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="BasicInfo"/></returns>
        public static async Task<BasicInfo> CreateAsync(string currentUserId, KeylolUser user, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            return new BasicInfo
            {
                Id = user.Id,
                HeaderImage = user.HeaderImage,
                AvatarImage = user.AvatarImage,
                UserName = user.UserName,
                GamerTag = user.GamerTag,
                RegisterTime = user.RegisterTime,
                IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Users.IsFriendAsync(currentUserId, user.Id),
                IsSubscribed = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, user.Id,
                        SubscriptionTargetType.User),
                FriendCount = await cachedData.Subscriptions.GetFriendCountAsync(user.Id),
                SubscribedUserCount = await cachedData.Subscriptions.GetSubscribedUserCountAsync(user.Id),
                SubscriberCount =
                    await cachedData.Subscriptions.GetSubscriberCountAsync(user.Id, SubscriptionTargetType.User),
                SteamProfileName = user.SteamProfileName
            };
        }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 页眉图片
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 玩家标签
        /// </summary>
        public string GamerTag { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegisterTime { get; set; }

        /// <summary>
        /// 是否是好友
        /// </summary>
        public bool? IsFriend { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? IsSubscribed { get; set; }

        /// <summary>
        /// 好友数
        /// </summary>
        public long? FriendCount { get; set; }

        /// <summary>
        /// 关注数
        /// </summary>
        public long? SubscribedUserCount { get; set; }

        /// <summary>
        /// 听众数
        /// </summary>
        public long? SubscriberCount { get; set; }

        /// <summary>
        /// Steam 昵称
        /// </summary>
        public string SteamProfileName { get; set; }
    }
}