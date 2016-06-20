using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.User
{
    /// <summary>
    /// 聚合 - 个人 - 轨道
    /// </summary>
    public class TimelinePage
    {
        /// <summary>
        /// 获取个人轨道页
        /// </summary>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> Get(string userIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, [Injected] KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if (user == null)
                return new TimelinePage();
            return await CreateAsync(user.Id, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取时间轴卡片列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="before">起始位置</param>
        /// <param name="take">获取数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelineCardList"/></returns>
        public static async Task<TimelineCardList> GetCards(string userId, int before, int take,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await TimelineCardList.CreateAsync(UserStream.Name(userId), StateTreeHelper.GetCurrentUserId(),
                take, true, dbContext, cachedData, before);
        }

        /// <summary>
        /// 创建 <see cref="TimelinePage"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> CreateAsync(string userId, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            return new TimelinePage
            {
                Cards = await TimelineCardList.CreateAsync(UserStream.Name(userId), currentUserId,
                    12, true, dbContext, cachedData)
            };
        }

        /// <summary>
        /// 卡片列表
        /// </summary>
        public TimelineCardList Cards { get; set; }
    }
}