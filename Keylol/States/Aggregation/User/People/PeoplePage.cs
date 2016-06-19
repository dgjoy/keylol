using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.User.People
{
    /// <summary>
    /// 聚合 - 个人 - 人脉
    /// </summary>
    public class PeoplePage
    {
        /// <summary>
        /// 获取指定用户的人脉页
        /// </summary>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="PeoplePage"/></returns>
        public static async Task<PeoplePage> Get(string userIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, [Injected] KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if (user == null)
                return new PeoplePage();
            return await CreateAsync(user.Id, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取用户好友列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="UserEntryList"/></returns>
        public static async Task<UserEntryList> GetFriends(string userId, int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return (await UserEntryList.CreateAsync(userId, StateTreeHelper.GetCurrentUserId(),
                UserRelationship.Friend, false, page, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 获取用户关注列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="UserEntryList"/></returns>
        public static async Task<UserEntryList> GetSubscribedUsers(string userId, int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return (await UserEntryList.CreateAsync(userId, StateTreeHelper.GetCurrentUserId(),
                UserRelationship.SubscribedUser, false, page, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 获取用户听众列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="UserEntryList"/></returns>
        public static async Task<UserEntryList> GetSubscribers(string userId, int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return (await UserEntryList.CreateAsync(userId, StateTreeHelper.GetCurrentUserId(),
                UserRelationship.Subscriber, false, page, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="PeoplePage"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PeoplePage"/></returns>
        public static async Task<PeoplePage> CreateAsync(string userId, string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var friends = await UserEntryList.CreateAsync(userId, currentUserId, UserRelationship.Friend, true, 1,
                dbContext, cachedData);
            var subscribedUsers = await UserEntryList.CreateAsync(userId, currentUserId,
                UserRelationship.SubscribedUser, true, 1, dbContext, cachedData);
            var subscribers = await UserEntryList.CreateAsync(userId, currentUserId, UserRelationship.Subscriber,
                true, 1, dbContext, cachedData);
            return new PeoplePage
            {
                FriendCount = friends.Item2,
                FriendPageCount = friends.Item3,
                Friends = friends.Item1,
                SubscribedUserCount = subscribedUsers.Item2,
                SubscribedUserPageCount = subscribedUsers.Item3,
                SubscribedUsers = subscribedUsers.Item1,
                SubscriberCount = subscribers.Item2,
                SubscriberPageCount = subscribers.Item3,
                Subscribers = subscribers.Item1
            };
        }

        /// <summary>
        /// 好友总数
        /// </summary>
        public int? FriendCount { get; set; }

        /// <summary>
        /// 好友总页数
        /// </summary>
        public int? FriendPageCount { get; set; }

        /// <summary>
        /// 好友列表
        /// </summary>
        public UserEntryList Friends { get; set; }

        /// <summary>
        /// 订阅的用户总数
        /// </summary>
        public int? SubscribedUserCount { get; set; }

        /// <summary>
        /// 订阅的用户总页数
        /// </summary>
        public int? SubscribedUserPageCount { get; set; }

        /// <summary>
        /// 订阅的用户列表
        /// </summary>
        public UserEntryList SubscribedUsers { get; set; }

        /// <summary>
        /// 听众总数
        /// </summary>
        public int? SubscriberCount { get; set; }

        /// <summary>
        /// 听众总页数
        /// </summary>
        public int? SubscriberPageCount { get; set; }

        /// <summary>
        /// 听众列表
        /// </summary>
        public UserEntryList Subscribers { get; set; }
    }
}