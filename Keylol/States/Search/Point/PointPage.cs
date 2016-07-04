using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search.Point
{
    /// <summary>
    /// 搜索 - 据点
    /// </summary>
    public class PointPage
    {
        /// <summary>
        /// 搜索据点页面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<PointPage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="PointResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<PointPage> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new PointPage
            {
                Results = await PointResultList.CreateAsync(currentUserId, keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 据点搜索列表
        /// </summary>
        public PointResultList Results { get; set; }
    }
}