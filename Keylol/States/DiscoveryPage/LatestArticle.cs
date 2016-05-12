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

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// 最新文章列表
    /// </summary>
    public class LatestArticleList : List<LatestArticle>
    {
        private const int RecordsPerPage = 10;

        private LatestArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定页码的最新文章列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>最新文章列表</returns>
        public static async Task<LatestArticleList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(page, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="LatestArticleList"/>
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="LatestArticleList"/></returns>
        public static async Task<LatestArticleList> CreateAsync(int page, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from article in dbContext.Articles
                where article.Rejected == false &&
                      article.Archived == ArchivedState.None &&
                      article.Deleted == DeletedState.None
                orderby article.Sid descending
                select new
                {
                    article.Id,
                    article.SidForAuthor,
                    article.Title,
                    article.PublishTime,
                    AuthorIdCode = article.Author.IdCode,
                    AuthorAvatarImage = article.Author.AvatarImage,
                    AuthorUserName = article.Author.UserName,
                    PointIdCode = article.TargetPoint.IdCode,
                    PointAvatarImage = article.TargetPoint.AvatarImage,
                    PointChineseName = article.TargetPoint.ChineseName,
                    PointEnglishName = article.TargetPoint.EnglishName
                }).TakePage(page, RecordsPerPage).ToListAsync();
            var result = new LatestArticleList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new LatestArticle
                {
                    LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Article),
                    CommentCount = await cachedData.ArticleComments.GetArticleCommentCount(a.Id),
                    SidForAuthor = a.SidForAuthor,
                    Title = a.Title,
                    PublishTime = a.PublishTime,
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    PointIdCode = a.PointIdCode,
                    PointAvatarImage = a.PointAvatarImage,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName
                });
            }
            return result;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        /// <returns>总页数</returns>
        public static async Task<int> PageCountAsync(KeylolDbContext dbContext)
        {
            return (int) Math.Ceiling(await (from article in dbContext.Articles
                where article.Rejected == false &&
                      article.Archived == ArchivedState.None &&
                      article.Deleted == DeletedState.None
                select article)
                .CountAsync()/(double) RecordsPerPage);
        }
    }

    /// <summary>
    /// 最新文章
    /// </summary>
    public class LatestArticle
    {
        /// <summary>
        /// 认可数
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 作者名下序号
        /// </summary>
        public int SidForAuthor { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }

        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorAvatarImage { get; set; }

        /// <summary>
        /// 作者昵称
        /// </summary>
        public string AuthorUserName { get; set; }

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