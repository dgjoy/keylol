using System;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Aggregation;
using Keylol.States.Aggregation.Point;
using Keylol.States.Entrance;
using Keylol.StateTreeManager;

namespace Keylol.States
{
    /// <summary>
    /// State Tree Root
    /// </summary>
    public class Root
    {
        /// <summary>
        /// 定位器名称
        /// </summary>
        public static string LocatorName() => "state";

        /// <summary>
        /// 获取新的完整状态树
        /// </summary>
        /// <param name="state">当前 UI 状态</param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="pointIdCode">据点识别码</param>
        /// <returns>完整状态树</returns>
        public static async Task<Root> Locate(string state, [Injected] KeylolUserManager userManager,
            [Injected] KeylolDbContext dbContext, [Injected] CouponProvider coupon,
            [Injected] CachedDataProvider cachedData, string pointIdCode = null)
        {
            var root = new Root();
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            if (await StateTreeHelper.CanAccessAsync<Root>(nameof(CurrentUser)))
            {
                var user = await userManager.FindByIdAsync(currentUserId);
                root.CurrentUser = await CurrentUser.CreateAsync(user, userManager, dbContext, coupon);
            }

            switch (state)
            {
                case "entrance":
                    root.Entrance = await EntranceLevel.CreateAsync(currentUserId,
                        States.Entrance.EntrancePage.Auto, dbContext, cachedData);
                    break;

                case "entrance.discovery":
                    root.Entrance = await EntranceLevel.CreateAsync(currentUserId,
                        States.Entrance.EntrancePage.Discovery, dbContext, cachedData);
                    break;

                case "entrance.points":
                    root.Entrance = await EntranceLevel.CreateAsync(currentUserId,
                        States.Entrance.EntrancePage.Points, dbContext, cachedData);
                    break;

                case "entrance.timeline":
                    root.Entrance = await EntranceLevel.CreateAsync(currentUserId,
                        States.Entrance.EntrancePage.Timeline, dbContext, cachedData);
                    break;

                case "aggregation.point":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.Auto, dbContext, cachedData)
                    };
                    break;

                case "aggregation.point.frontpage":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.Frontpage, dbContext, cachedData)
                    };
                    break;

                case "aggregation.point.intel":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.Intel, dbContext, cachedData)
                    };
                    break;

                case "aggregation.point.timeline":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.Timeline, dbContext, cachedData)
                    };
                    break;

                case "aggregation.point.edit.info":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.EditInfo, dbContext, cachedData)
                    };
                    break;

                case "aggregation.point.edit.style":
                    root.Aggregation = new AggregationLevel
                    {
                        Point = await PointLevel.CreateAsync(currentUserId, pointIdCode,
                            States.Aggregation.Point.EntrancePage.EditStyle, dbContext, cachedData)
                    };
                    break;

                default:
                    throw new NotSupportedException("Not supported state.");
            }
            return root;
        }

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        [Authorize]
        public CurrentUser CurrentUser { get; set; }

        /// <summary>
        /// 入口层级
        /// </summary>
        public EntranceLevel Entrance { get; set; }

        /// <summary>
        /// 聚合层级
        /// </summary>
        public AggregationLevel Aggregation { get; set; }

        /// <summary>
        /// 待开设据点
        /// </summary>
        [Authorize]
        public PointToCreate PointToCreate { get; set; }
    }
}