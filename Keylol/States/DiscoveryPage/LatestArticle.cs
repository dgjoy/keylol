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
            return (await CreateAsync(page, false, false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="LatestArticleList"/>
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="returnPageCount">是否返回总页数</param>
        /// <param name="returnFirstCoverImage">是否返回第一篇文章封面图</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="LatestArticleList"/>，Item2 表示总页数，Item3 表示第一篇文章封面图</returns>
        public static async Task<Tuple<LatestArticleList, int, string>> CreateAsync(int page, bool returnPageCount,
            bool returnFirstCoverImage, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from article in dbContext.Articles
                where article.Rejected == false &&
                      article.Archived == ArchivedState.None &&
                      article.Deleted == DeletedState.None
                orderby article.Sid descending
                select article;
            var queryResult = await conditionQuery.Select(a => new
            {
                TotalCount = returnPageCount ? conditionQuery.Count() : 1,
                CoverImage = returnFirstCoverImage ? a.CoverImage : null,
                a.Id,
                a.SidForAuthor,
                a.Title,
                a.PublishTime,
                AuthorIdCode = a.Author.IdCode,
                AuthorAvatarImage = a.Author.AvatarImage,
                AuthorUserName = a.Author.UserName,
                PointIdCode = a.TargetPoint.IdCode,
                PointAvatarImage = a.TargetPoint.AvatarImage,
                PointChineseName = a.TargetPoint.ChineseName,
                PointEnglishName = a.TargetPoint.EnglishName
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
            var firstRecord = queryResult.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.CoverImage));
            return new Tuple<LatestArticleList, int, string>(
                result,
                (int) Math.Ceiling(firstRecord?.TotalCount/(double) RecordsPerPage ?? 1),
                firstRecord?.CoverImage);
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