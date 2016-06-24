using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.States.Entrance.Points
{
    /// <summary>
    /// 精选用户列表
    /// </summary>
    public class SpotlightUserList : List<SpotlightUser>
    {
        private SpotlightUserList([NotNull] IEnumerable<SpotlightUser> collection) : base(collection)
        {
        }

        /// <summary>
        /// 获取精选用户列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SpotlightUserList"/></returns>
        public static async Task<SpotlightUserList> Get(int page, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="SpotlightUserList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SpotlightUserList"/></returns>
        public static async Task<SpotlightUserList> CreateAsync(string currentUserId, int page,
            KeylolDbContext dbContext)
        {
            var query = string.IsNullOrWhiteSpace(currentUserId)
                ? from user in dbContext.Users
                    orderby dbContext.Subscriptions.Count(
                        s => s.TargetId == user.Id && s.TargetType == SubscriptionTargetType.User) descending
                    select new
                    {
                        user.Id,
                        user.IdCode,
                        user.HeaderImage,
                        user.AvatarImage,
                        user.UserName
                    }
                : from record in dbContext.UserSteamFriendRecords
                    where record.UserId == currentUserId
                    join login in dbContext.Set<IdentityUserLogin>() on record.FriendSteamId equals login.ProviderKey
                    where login.LoginProvider == KeylolLoginProviders.Steam
                    join user in dbContext.Users on login.UserId equals user.Id
                    where !dbContext.Subscriptions.Any(s => s.SubscriberId == currentUserId &&
                                                            s.TargetId == user.Id &&
                                                            s.TargetType == SubscriptionTargetType.User)
                    orderby dbContext.Subscriptions.Count(
                        s => s.TargetId == user.Id && s.TargetType == SubscriptionTargetType.User) descending
                    select new
                    {
                        user.Id,
                        user.IdCode,
                        user.HeaderImage,
                        user.AvatarImage,
                        user.UserName
                    };
            return new SpotlightUserList((await query.TakePage(page, 12).ToListAsync()).Select(u => new SpotlightUser
            {
                Id = u.Id,
                IdCode = u.IdCode,
                HeaderImage = u.HeaderImage,
                AvatarImage = u.AvatarImage,
                UserName = u.UserName
            }));
        }
    }

    /// <summary>
    /// 精选用户
    /// </summary>
    public class SpotlightUser
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头部图
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string UserName { get; set; }
    }
}