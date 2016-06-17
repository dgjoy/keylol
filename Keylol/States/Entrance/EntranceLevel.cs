using System;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Entrance.Discovery;
using Keylol.States.Entrance.Points;
using Keylol.States.Entrance.Timeline;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Entrance
{
    /// <summary>
    /// 入口层级
    /// </summary>
    public class EntranceLevel
    {
        /// <summary>
        /// 获取“入口”层级状态分支
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="EntranceLevel"/></returns>
        public static async Task<EntranceLevel> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), EntrancePage.Auto,
                dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="EntranceLevel"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="targetPage">要获取的页面</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetPage"/> 超出范围</exception>
        public static async Task<EntranceLevel> CreateAsync(string currentUserId, EntrancePage targetPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var result = new EntranceLevel();
            switch (targetPage)
            {
                case EntrancePage.Auto:
//                    if (string.IsNullOrWhiteSpace(currentUserId))
//                    {
//                        result.Discovery = await DiscoveryPage.CreateAsync(currentUserId, dbContext, cachedData);
//                        result.Current = EntrancePage.Discovery;
//                    }
//                    else
//                    {
//                        result.Timeline = await TimelinePage.CreateAsync(currentUserId, dbContext, cachedData);
//                        result.Current = EntrancePage.Timeline;
//                    }
                    result.Discovery = await DiscoveryPage.CreateAsync(currentUserId, dbContext, cachedData);
                    result.Current = EntrancePage.Discovery;
                    break;

                case EntrancePage.Discovery:
                    result.Discovery = await DiscoveryPage.CreateAsync(currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Points:
                    result.Points = await PointsPage.CreateAsync(currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Timeline:
                    result.Timeline = await TimelinePage.CreateAsync(currentUserId, dbContext, cachedData);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetPage), targetPage, null);
            }
            return result;
        }

        /// <summary>
        /// 当前页面
        /// </summary>
        public EntrancePage? Current { get; set; }

        /// <summary>
        /// 发现
        /// </summary>
        public DiscoveryPage Discovery { get; set; }

        /// <summary>
        /// 据点
        /// </summary>
        public PointsPage Points { get; set; }

        /// <summary>
        /// 轨道
        /// </summary>
        public TimelinePage Timeline { get; set; }
    }

    /// <summary>
    /// 目标入口页
    /// </summary>
    public enum EntrancePage
    {
        /// <summary>
        /// 自动（根据登录状态）
        /// </summary>
        Auto,

        /// <summary>
        /// 发现
        /// </summary>
        Discovery,

        /// <summary>
        /// 据点
        /// </summary>
        Points,

        /// <summary>
        /// 轨道
        /// </summary>
        Timeline
    }
}