using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.User.Dossier.Article
{
    /// <summary>
    /// 文章页面
    /// </summary>
    public class ArticlePage
    {
        /// <summary>
        /// 用户文章列表页
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<ArticlePage> Get(string userId, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(userId, page, recordsPerPage, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="ArticlePage"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<ArticlePage> CreateAsync(string userId, int page, int recordsPerPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            return new ArticlePage
            {
                Results = await ArticleList.CreateAsync(userId,page,recordsPerPage,dbContext,cachedData)
            };
        }
        /// <summary>
        /// 用户所有文章
        /// </summary>
        public ArticleList Results { get; set; }
    }
}