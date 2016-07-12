using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point
{
    /// <summary>
    /// 聚合 - 据点 - 轨道
    /// </summary>
    public class TimelinePage
    {
        /// <summary>
        /// 获取据点轨道页
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new TimelinePage();
            return await CreateAsync(point.Id, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取时间轴卡片列表
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="before">起始位置</param>
        /// <param name="take">获取数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelineCardList"/></returns>
        public static async Task<TimelineCardList> GetCards(string pointId, int before, int take,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await TimelineCardList.CreateAsync(PointStream.Name(pointId), StateTreeHelper.GetCurrentUserId(),
                take, false, dbContext, cachedData, before);
        }

        /// <summary>
        /// 创建 <see cref="TimelinePage"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="TimelinePage"/></returns>
        public static async Task<TimelinePage> CreateAsync(string pointId, string currentUserId,
            KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            return new TimelinePage
            {
                Cards = await TimelineCardList.CreateAsync(PointStream.Name(pointId), currentUserId,
                    18, false, dbContext, cachedData)
            };
        }

        /// <summary>
        /// 卡片列表
        /// </summary>
        public TimelineCardList Cards { get; set; }
    }
}