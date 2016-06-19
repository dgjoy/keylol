using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 文选文章列表
    /// </summary>
    public class SelectedArticleList : List<SelectedArticle>
    {
        private SelectedArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定用户的文选
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns></returns>
        public static async Task<SelectedArticleList> Get(string userId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return (await CreateAsync(userId, page, 12, false, StateTreeHelper.GetCurrentUserId(), dbContext,
                cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="SelectedArticleList"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="SelectedArticleList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<SelectedArticleList, int>> CreateAsync(string userId, int page,
            int recordsPerPage, bool returnCount, string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from article in dbContext.Articles
                where article.AuthorId == userId
                orderby dbContext.Likes
                    .Count(l => l.TargetId == article.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    Count = returnCount ? dbContext.Articles.Count(a => a.AuthorId == userId) : 1,
                    article.SidForAuthor,
                    article.Title,
                    article.Subtitle,
                    article.CoverImage,
                    PointIdCode = article.TargetPoint.IdCode,
                    PointAvatarImage = article.TargetPoint.AvatarImage,
                    PointChineseName = article.TargetPoint.ChineseName,
                    PointEnglishName = article.TargetPoint.EnglishName,
                    PointSteamAppId = article.TargetPoint.SteamAppId
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new SelectedArticleList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new SelectedArticle
                {
                    SidForAuthor = a.SidForAuthor,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    CoverImage = a.CoverImage,
                    PointIdCode = a.PointIdCode,
                    PointAvatarImage = a.PointAvatarImage,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName,
                    PointInLibrary = string.IsNullOrWhiteSpace(currentUserId) || a.PointSteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, a.PointSteamAppId.Value)
                });
            }

            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<SelectedArticleList, int>(result, firstRecord?.Count ?? 0);
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

        /// <summary>
        /// 收稿据点是否已入库
        /// </summary>
        public bool? PointInLibrary { get; set; }
    }
}