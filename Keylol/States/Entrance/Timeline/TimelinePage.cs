using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Entrance.Timeline
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
        /// <returns>是日优惠据点列表</returns>
        public static async Task<TimelinePage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
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
                Test = "测试属性"
            };
        }

        /// <summary>
        /// 测试属性
        /// </summary>
        public string Test { get; set; }
    }
}