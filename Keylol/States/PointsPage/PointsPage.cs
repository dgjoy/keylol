using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;

namespace Keylol.States.PointsPage
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
        /// <returns>是日优惠据点列表</returns>
        public static async Task<PointsPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.CurrentUser().Identity.GetUserId(), dbContext, cachedData);
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
            return new PointsPage
            {
                TestProperty = "测试属性"
            };
        }

        /// <summary>
        /// 测试属性
        /// </summary>
        public string TestProperty { get; set; }
    }
}