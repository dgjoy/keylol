using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search.User
{
    /// <summary>
    /// 搜索 - 用户
    /// </summary>
    public class UserPage
    {
        /// <summary>
        /// 搜索用户页面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部搜索</param>
        public static async Task<UserPage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="UserPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<UserPage> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new UserPage
            {
                Results = await UserResultList.CreateAsync(currentUserId, keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 用户搜索列表
        /// </summary>
        public UserResultList Results { get; set; }
    }
}