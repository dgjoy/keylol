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

namespace Keylol.States.Aggregation.Point.Frontpage
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
        /// 获取指定据点的最新文章列表
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="LatestArticleList"/></returns>
        public static async Task<LatestArticleList> Get(string pointId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return (await CreateAsync(pointId, page, false, false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="LatestArticleList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="returnPageCount">是否返回总页数</param>
        /// <param name="returnFirstCoverImage">是否返回第一篇文章封面图</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="LatestArticleList"/>，Item2 表示总页数，Item3 表示第一篇文章封面图</returns>
        public static async Task<Tuple<LatestArticleList, int, string>> CreateAsync(string pointId, int page,
            bool returnPageCount, bool returnFirstCoverImage, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var streamName = PointStream.Name(pointId);
            var conditionQuery = from feed in dbContext.Feeds
                where feed.StreamName == streamName && feed.EntryType == FeedEntryType.ArticleId
                join article in dbContext.Articles on feed.Entry equals article.Id
                where article.Archived == ArchivedState.None && article.Rejected == false
                orderby feed.Id descending
                select article;
            var queryResult = await conditionQuery.Select(a => new
            {
                Count = returnPageCount ? conditionQuery.Count() : 1,
                CoverImage = returnFirstCoverImage ? a.CoverImage : null,
                a.Id,
                a.SidForAuthor,
                a.Title,
                a.PublishTime,
                AuthorIdCode = a.Author.IdCode,
                AuthorAvatarImage = a.Author.AvatarImage,
                AuthorUserName = a.Author.UserName,
                PointIdCode = a.TargetPoint.IdCode,
                PointType = a.TargetPoint.Type,
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
                    CommentCount = await cachedData.ArticleComments.GetArticleCommentCountAsync(a.Id),
                    SidForAuthor = a.SidForAuthor,
                    Title = a.Title,
                    PublishTime = a.PublishTime,
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    PointIdCode = a.PointIdCode,
                    PointType = a.PointType,
                    PointAvatarImage = a.PointAvatarImage,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName
                });
            }
            var firstRecord = queryResult.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.CoverImage));
            return new Tuple<LatestArticleList, int, string>(
                result,
                (int) Math.Ceiling(firstRecord?.Count/(double) RecordsPerPage ?? 1),
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
        /// 收稿据点类型
        /// </summary>
        public PointType PointType { get; set; }

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