using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search.Article
{
    /// <summary>
    /// 搜索 - 文章
    /// </summary>
    public class ArticlePage
    {
        /// <summary>
        /// 搜索文章界面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否查询全部</param>
        public static async Task<ArticlePage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return await CreateAsync(keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="ArticleResultList"/>
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否查询全部</param>
        public static async Task<ArticlePage> CreateAsync(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new ArticlePage
            {
                Results = await ArticleResultList.CreateAsync(keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 文章搜索结果
        /// </summary>
        public ArticleResultList Results { get; set; }
    }
}