using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Identity;
using Keylol.Models;
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
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"></param>
        public static async Task<ArticlePage> Get(string userIdCode, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData,
            [Injected] KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if(user == null)
                return new ArticlePage();
            return await CreateAsync(user.Id, page, recordsPerPage, dbContext, cachedData);
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
            var articles = await ArticleList.CreateAsync(userId, page, recordsPerPage, true, dbContext, cachedData);

            return new ArticlePage
            {
                Results = articles.Item1,
                ArticleCount = articles.Item2
            };
        }

        /// <summary>
        /// 用户所有文章
        /// </summary>
        public ArticleList Results { get; set; }

        /// <summary>
        /// 用户文章总数
        /// </summary>
        public int? ArticleCount { get; set; }
    }
}