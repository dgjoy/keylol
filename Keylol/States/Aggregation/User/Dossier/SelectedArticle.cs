using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 文选文章列表
    /// </summary>
    public class SelectedArticleList : List<SelectedArticle>
    {
        /// <summary>
        /// 创建 <see cref="SelectedArticleList"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="SelectedArticleList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<SelectedArticleList, int>> CreateAsync(string userId, int page,
            int recordsPerPage, bool returnCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 文选文章
    /// </summary>
    public class SelectedArticle
    {
        /// <summary>
        /// 作者名下序号
        /// </summary>
        public int SidForAuthor { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 收稿据点识别码
        /// </summary>
        public string PointIdCode { get; set; }

        /// <summary>
        /// 收稿据点头像
        /// </summary>
        public string PointAvatarImage { get; set; }

        /// <summary>
        /// 收稿据点中文名
        /// </summary>
        public string PointChineseName { get; set; }

        /// <summary>
        /// 收稿据点英文名
        /// </summary>
        public string PointEnglishName { get; set; }
    }
}