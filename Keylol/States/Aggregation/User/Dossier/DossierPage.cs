using System.Data.Entity;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 聚合 - 个人 - 档案
    /// </summary>
    public class DossierPage
    {
        /// <summary>
        /// 创建 <see cref="DossierPage"/>
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="DossierPage"/></returns>
        public static async Task<DossierPage> CreateAsync(KeylolUser user, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData, KeylolUserManager userManager)
        {
            var subscribedPoints = await SubscribedPointList.CreateAsync(user.Id, 1, 3, true, dbContext);
            var dossierPage = new DossierPage
            {
                Coupon = user.Coupon,
                LikeCount = await cachedData.Likes.GetUserLikeCountAsync(user.Id),
                GameCount = await dbContext.UserSteamGameRecords.CountAsync(r => r.UserId == user.Id),
                PlayedGameCount =
                    await dbContext.UserSteamGameRecords.CountAsync(r => r.UserId == user.Id && r.TotalPlayedTime > 0),
                SpotlightCount = await dbContext.Articles.CountAsync(a => a.AuthorId == user.Id && a.Spotlighted),
                IsOperator = await userManager.IsInRoleAsync(user.Id, KeylolRoles.Operator),
                SubscribedPointCount = subscribedPoints.Item2,
                SubscribedPoints = subscribedPoints.Item1
            };
            return dossierPage;
        }

        /// <summary>
        /// 文券数
        /// </summary>
        public int? Coupon { get; set; }

        /// <summary>
        /// 获得认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 库内游戏数
        /// </summary>
        public int? GameCount { get; set; }

        /// <summary>
        /// 记录在案游戏数
        /// </summary>
        public int? PlayedGameCount { get; set; }

        /// <summary>
        /// 萃选数
        /// </summary>
        public int? SpotlightCount { get; set; }

        /// <summary>
        /// 是否是站务职员
        /// </summary>
        public bool? IsOperator { get; set; }

        /// <summary>
        /// 订阅的据点数
        /// </summary>
        public int? SubscribedPointCount { get; set; }

        /// <summary>
        /// 订阅的据点列表
        /// </summary>
        public SubscribedPointList SubscribedPoints { get; set; }
    }
}