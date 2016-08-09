using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.Dossier.Article
{
    /// <summary>
    /// 文章列表
    /// </summary>
    public class ArticleList : List<Article>
    {
        private ArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取用户文章列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<ArticleList> Get(string userId, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return (await CreateAsync(userId, page, recordsPerPage, false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建用户文章列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="ArticleList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<ArticleList, int>> CreateAsync(string userId, int page, int recordsPerPage,
            bool returnCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from article in dbContext.Articles
                where article.AuthorId == userId
                orderby article.PublishTime
                    descending
                select new
                {
                    article.Id,
                    article.Title,
                    article.Subtitle,
                    article.SidForAuthor,
                    article.Archived,
                    article.PublishTime,
                    PointIdCode = article.TargetPoint.IdCode,
                    PointChineseName = article.TargetPoint.ChineseName,
                    PointEnglishName = article.TargetPoint.EnglishName,
                    PointAvatarImage = article.TargetPoint.AvatarImage,
                    Count = returnCount ? dbContext.Articles.Count(a => a.AuthorId == userId) : 1
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new ArticleList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new Article
                {
                    Title = a.Title,
                    SubTitle = a.Subtitle,
                    SidForAuthor = a.SidForAuthor,
                    LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Article),
                    CommentCount = await cachedData.ArticleComments.GetArticleCommentCountAsync(a.Id),
                    ArchivedState = a.Archived,
                    PublishTime = a.PublishTime,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName,
                    PointIdCode = a.PointIdCode,
                    PointAvatarImage = a.PointAvatarImage,
                });
            }

            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<ArticleList, int>(result, firstRecord?.Count ?? 0);
        }
    }

    /// <summary>
    /// 文章
    /// </summary>
    public class Article
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 子标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 文章在作者名下序号
        /// </summary>
        public int? SidForAuthor { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 文章状态
        /// </summary>
        public ArchivedState ArchivedState { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 文章所属据点 IdCode
        /// </summary>
        public string PointIdCode { get; set; }

        /// <summary>
        /// 文章所属据点中文名
        /// </summary>
        public string PointChineseName { get; set; }

        /// <summary>
        /// 文章所属据点英文名
        /// </summary>
        public string PointEnglishName { get; set; }

        /// <summary>
        /// 文章所属据点头像
        /// </summary>
        public string PointAvatarImage { get; set; }


    }
}