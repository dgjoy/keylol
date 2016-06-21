using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Content.Article
{
    /// <summary>
    /// 文章评论列表
    /// </summary>
    public class ArticleCommentList : List<ArticleComment>
    {
        private const int RecordsPerPage = 10;

        private ArticleCommentList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定文章的评论列表
        /// </summary>
        /// <param name="articleId">文章 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ArticleCommentList"/></returns>
        public static async Task<ArticleCommentList> Get(string articleId, int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            var article = await dbContext.Articles
                .Include(a => a.Author)
                .Include(a => a.TargetPoint)
                .Where(a => a.Id == articleId)
                .SingleOrDefaultAsync();

            if (article == null)
                return new ArticleCommentList(0);

            return (await CreateAsync(article, page, StateTreeHelper.GetCurrentUserId(),
                StateTreeHelper.GetCurrentUser().IsInRole(KeylolRoles.Operator), false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="ArticleCommentList"/>
        /// </summary>
        /// <param name="article">文章对象</param>
        /// <param name="page">分页页码</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="isOperator">当前登录用户是否为运维职员</param>
        /// <param name="returnMeta">是否返回元数据（总页数、总评论数、最新评论时间）</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="ArticleCommentList"/>， Item2 表示总评论数，Item3 表示最新评论时间，Item4 表示总页数</returns>
        public static async Task<Tuple<ArticleCommentList, int, DateTime?, int>> CreateAsync(Models.Article article,
            int page, string currentUserId, bool isOperator, bool returnMeta, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from comment in dbContext.ArticleComments
                where comment.ArticleId == article.Id
                orderby comment.Sid
                select new
                {
                    Author = comment.Commentator,
                    comment.Id,
                    comment.PublishTime,
                    comment.SidForArticle,
                    comment.Content,
                    comment.Archived,
                    comment.Warned
                }).TakePage(page, RecordsPerPage).ToListAsync();

            var result = new ArticleCommentList(queryResult.Count);
            foreach (var c in queryResult)
            {
                var articleComment = new ArticleComment
                {
                    Id = c.Id,
                    SidForArticle = c.SidForArticle,
                    Archived = c.Archived != ArchivedState.None
                };
                // ReSharper disable once PossibleInvalidOperationException
                if (!articleComment.Archived.Value || currentUserId == c.Author.Id || isOperator)
                {
                    articleComment.AuthorIdCode = c.Author.IdCode;
                    articleComment.AuthorAvatarImage = c.Author.AvatarImage;
                    articleComment.AuthorUserName = c.Author.UserName;
                    articleComment.AuthorPlayedTime = article.TargetPoint.SteamAppId == null
                        ? null
                        : (await dbContext.UserSteamGameRecords
                            .Where(r => r.UserId == c.Author.Id && r.SteamAppId == article.TargetPoint.SteamAppId)
                            .SingleOrDefaultAsync())?.TotalPlayedTime;
                    articleComment.LikeCount =
                        await cachedData.Likes.GetTargetLikeCountAsync(c.Id, LikeTargetType.ArticleComment);
                    articleComment.Liked = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Likes.IsLikedAsync(currentUserId, c.Id, LikeTargetType.ArticleComment);
                    articleComment.PublishTime = c.PublishTime;
                    articleComment.Content = c.Content;
                    articleComment.Warned = c.Warned;
                }
                result.Add(articleComment);
            }
            var latestCommentTime = returnMeta
                ? await (from comment in dbContext.ArticleComments
                    where comment.ArticleId == article.Id
                    orderby comment.Sid descending
                    select comment.PublishTime).FirstOrDefaultAsync()
                : default(DateTime);
            var count = await cachedData.ArticleComments.GetArticleCommentCountAsync(article.Id);
            return new Tuple<ArticleCommentList, int, DateTime?, int>(result,
                count,
                latestCommentTime == default(DateTime) ? (DateTime?) null : latestCommentTime,
                count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1);
        }
    }

    /// <summary>
    /// 文章评论
    /// </summary>
    public class ArticleComment
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorAvatarImage { get; set; }

        /// <summary>
        /// 作者用户名
        /// </summary>
        public string AuthorUserName { get; set; }

        /// <summary>
        /// 作者在档时间
        /// </summary>
        public double? AuthorPlayedTime { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 楼层号
        /// </summary>
        public int SidForArticle { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否被封存
        /// </summary>
        public bool? Archived { get; set; }

        /// <summary>
        /// 是否被警告
        /// </summary>
        public bool? Warned { get; set; }
    }
}