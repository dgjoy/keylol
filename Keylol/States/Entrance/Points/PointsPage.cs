using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Entrance.Points
{
    /// <summary>
    /// 入口 - 据点
    /// </summary>
    public class PointsPage
    {
        /// <summary>
        /// 获取“入口 - 据点”页面状态分支
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointsPage"/></returns>
        public static async Task<PointsPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="PointsPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointsPage"/></returns>
        public static async Task<PointsPage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var recentPlayedPoints = await RecentPlayedPointList.CreateAsync(currentUserId, dbContext, cachedData);
            var recentPoints = await RecentPointList.CreateAsync(currentUserId, 1, true, dbContext, cachedData);
            return new PointsPage
            {
                OutpostPoints = await OutpostPointList.CreateAsync(currentUserId, 1, 15, dbContext, cachedData),
                RecentPlayedPointHeaderImage = recentPlayedPoints.Item2,
                RecentPlayedPoints = recentPlayedPoints.Item1,
                InterestedPoints = await InterestedPointList.CreateAsync(currentUserId, 1, dbContext),
                SpotlightUsers = await SpotlightUserList.CreateAsync(currentUserId, 1, dbContext),
                RecentPointPageCount = recentPoints.Item2,
                RecentPoints = recentPoints.Item1
            };
        }

        /// <summary>
        /// 哨所据点
        /// </summary>
        public OutpostPointList OutpostPoints { get; set; }

        /// <summary>
        /// 最近玩过的游戏对应据点列表头部图
        /// </summary>
        public string RecentPlayedPointHeaderImage { get; set; }

        /// <summary>
        /// 最近玩过的游戏对应据点
        /// </summary>
        public RecentPlayedPointList RecentPlayedPoints { get; set; }

        /// <summary>
        /// 可能感兴趣的据点
        /// </summary>
        public InterestedPointList InterestedPoints { get; set; }

        /// <summary>
        /// 精选用户
        /// </summary>
        public SpotlightUserList SpotlightUsers { get; set; }

        /// <summary>
        /// 最近有动态的据点总页数
        /// </summary>
        public int RecentPointPageCount { get; set; }

        /// <summary>
        /// 最近有动态的据点
        /// </summary>
        public RecentPointList RecentPoints { get; set; }
    }
}