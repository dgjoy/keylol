using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States.Entrance
{
    /// <summary>
    /// 入口 - 轨道
    /// </summary>
    public class TimelinePage
    {
        /// <summary>
        /// 获取“入口 - 轨道”页面状态分支
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取时间轴卡片列表
        /// </summary>
        /// <param name="before">起始位置</param>
        /// <param name="take">获取数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelineCardList"/></returns>
        public static async Task<TimelineCardList> GetCards(int before, int take, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await TimelineCardList.CreateAsync(SubscriptionStream.Name(currentUserId), currentUserId,
                take, dbContext, cachedData, before);
        }

        /// <summary>
        /// 创建 <see cref="TimelinePage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            return new TimelinePage
            {
                Cards = await TimelineCardList.CreateAsync(SubscriptionStream.Name(currentUserId), currentUserId,
                    12, dbContext, cachedData)
            };
        }

        /// <summary>
        /// 卡片列表
        /// </summary>
        public TimelineCardList Cards { get; set; }
    }
}