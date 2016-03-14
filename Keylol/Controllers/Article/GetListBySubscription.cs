using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using StackExchange.Redis;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取当前用户主订阅时间轴的文章
        /// </summary>
        /// <param name="filter">文章过滤器，默认 0</param>
        /// <param name="before">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("subscription")]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetBySubscription(int filter = 0, int before = int.MaxValue, int take = 30)
        {
            const int timelineCapacity = 100;
            var redisDb = RedisProvider.GetInstance().GetDatabase();
            var userId = User.Identity.GetUserId();
            var cacheKey = $"user:{userId}:subscription.timeline:{filter}";
            if (take > 50) take = 50;

            var afterValue = await redisDb.ListGetByIndexAsync(cacheKey, 0);
            var after = afterValue.HasValue ? (int)afterValue : 0;

            var user = await DbContext.Users.Where(u => u.Id == userId).SingleAsync();
            var results = new Dictionary<int, ArticleDTO>();

            // 自动订阅的据点
            foreach (var entry in await DbContext.AutoSubscriptions.Where(s => s.UserId == userId)
                .SelectMany(s => s.NormalPoint.Articles.Select(a => new {article = a, fromPoint = s.NormalPoint}))
                .Where(e => e.article.SequenceNumber < before && e.article.SequenceNumber > after)
                .OrderByDescending(e => e.article.SequenceNumber)
                .Take(() => timelineCapacity)
                .ToListAsync())
            {
                if (results.ContainsKey(entry.article.SequenceNumber))
                {
                    results[entry.article.SequenceNumber].AttachedPoints.Add(new NormalPointDTO(entry.fromPoint, true));
                }
                else
                {
                    results[entry.article.SequenceNumber] = new ArticleDTO(entry.article, true, 256, true)
                    {
                        TimelineReason = ArticleDTO.TimelineReasonType.Point,
                        AttachedPoints = new List<NormalPointDTO> {new NormalPointDTO(entry.fromPoint, true)}
                    };
                }
            }

            // 手动订阅的据点
            foreach (var entry in await DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.SubscribedPoints.OfType<Models.NormalPoint>())
                .SelectMany(p => p.Articles.Select(a => new {article = a, fromPoint = p}))
                .Where(e => e.article.SequenceNumber < before && e.article.SequenceNumber > after)
                .OrderByDescending(e => e.article.SequenceNumber)
                .Take(() => timelineCapacity)
                .ToListAsync())
            {
                if (results.ContainsKey(entry.article.SequenceNumber))
                {
                    var overrideArticle = results[entry.article.SequenceNumber];
                    overrideArticle.TimelineReason = ArticleDTO.TimelineReasonType.Point;
                    overrideArticle.AttachedPoints.Add(new NormalPointDTO(entry.fromPoint, true));
                }
                else
                {
                    results[entry.article.SequenceNumber] = new ArticleDTO(entry.article, true, 256, true)
                    {
                        TimelineReason = ArticleDTO.TimelineReasonType.Point,
                        AttachedPoints = new List<NormalPointDTO> {new NormalPointDTO(entry.fromPoint, true)}
                    };
                }
            }

            // 订阅的用户认可的文章
            foreach (var like in await DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>())
                .SelectMany(p => p.User.Likes.OfType<ArticleLike>())
                .Where(l => l.Backout == false &&
                            l.Article.SequenceNumber < before && l.Article.SequenceNumber > after)
                .OrderByDescending(l => l.Article.SequenceNumber)
                .Take(() => timelineCapacity)
                .ToListAsync())
            {
                if (results.ContainsKey(like.Article.SequenceNumber))
                {
                    var overrideArticle = results[like.Article.SequenceNumber];
                    overrideArticle.TimelineReason = ArticleDTO.TimelineReasonType.Like;
                    if (overrideArticle.AttachedPoints != null)
                        overrideArticle.AttachedPoints = null;
                }
                else
                {
                    results[like.Article.SequenceNumber] = new ArticleDTO(like.Article, true, 256, true)
                    {
                        TimelineReason = ArticleDTO.TimelineReasonType.Like
                    };
                }
            }

            // 订阅的用户发表的文章
            foreach (var article in await DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>())
                .SelectMany(p => p.Entries.OfType<Models.Article>())
                .Where(a => a.SequenceNumber < before && a.SequenceNumber > after)
                .OrderByDescending(a => a.SequenceNumber)
                .Take(() => timelineCapacity)
                .ToListAsync())
            {
                if (results.ContainsKey(article.SequenceNumber))
                {
                    var overrideArticle = results[article.SequenceNumber];
                    overrideArticle.TimelineReason = ArticleDTO.TimelineReasonType.Publish;
                    if (overrideArticle.AttachedPoints != null)
                        overrideArticle.AttachedPoints = null;
                }
                else
                {
                    results[article.SequenceNumber] = new ArticleDTO(article, true, 256, true)
                    {
                        TimelineReason = ArticleDTO.TimelineReasonType.Publish
                    };
                }
            }
            /*
            var userQuery = DbContext.Users.AsNoTracking().Where(u => u.Id == userId);
                var profilePointsQuery = userQuery.SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>());

                var articleQuery =
                    userQuery.SelectMany(u => u.SubscribedPoints.OfType<Models.NormalPoint>())
                        .SelectMany(p => p.Articles.Select(a => new {article = a, fromPoint = p}))
                        .Where(e => e.article.SequenceNumber < beforeSN && e.article.Type.Name != "简评")
                        .Select(e => new
                        {
                            e.article,
                            e.fromPoint,
                            reason = ArticleDTO.TimelineReasonType.Point,
                            likedByUser = (KeylolUser) null
                        })
                        .Concat(profilePointsQuery.SelectMany(p => p.Entries.OfType<Models.Article>())
                            .Where(a => a.SequenceNumber < beforeSN)
                            .Select(a => new
                            {
                                article = a,
                                fromPoint = (Models.NormalPoint) null,
                                reason = ArticleDTO.TimelineReasonType.Publish,
                                likedByUser = (KeylolUser) null
                            }))
                        .Concat(profilePointsQuery.Select(p => p.User)
                            .SelectMany(u => u.Likes.OfType<ArticleLike>())
                            .Where(l => l.Backout == false && l.Article.SequenceNumber < beforeSN)
                            .Select(l => new
                            {
                                article = l.Article,
                                fromPoint = (Models.NormalPoint) null,
                                reason = ArticleDTO.TimelineReasonType.Like,
                                likedByUser = l.Operator
                            }))
                        .Concat(dbContext.AutoSubscriptions.Where(s => s.UserId == userId)
                            .SelectMany(
                                s => s.NormalPoint.Articles.Select(a => new {article = a, fromPoint = s.NormalPoint}))
                            .Where(e => e.article.SequenceNumber < beforeSN && e.article.Type.Name != "简评")
                            .Select(e => new
                            {
                                e.article,
                                e.fromPoint,
                                reason = ArticleDTO.TimelineReasonType.Point,
                                likedByUser = (KeylolUser) null
                            }));

                if (articleTypeFilter != null)
                {
                    var typesName = articleTypeFilter.Split(',').Select(s => s.Trim()).ToList();
                    articleQuery = articleQuery.Where(PredicateBuilder.Contains(typesName, a => a.article.Type.Name, new
                    {
                        article = (Models.Article) null,
                        fromPoint = (Models.NormalPoint) null,
                        reason = ArticleDTO.TimelineReasonType.Like,
                        likedByUser = (KeylolUser) null
                    }));
                }

                var articleEntries = await articleQuery.GroupBy(e => e.article)
                    .OrderByDescending(g => g.Key.SequenceNumber).Take(() => take)
                    .Select(g => new
                    {
                        article = g.Key,
                        likedByUsers = g.Where(e => e.reason == ArticleDTO.TimelineReasonType.Like)
                            .Take(3)
                            .Select(e => e.likedByUser),
                        fromPoints = g.Where(e => e.fromPoint != null).Select(e => e.fromPoint),
                        reason = g.Max(ee => ee.reason)
                    })
                    .Select(g => new
                    {
                        g.article,
                        g.reason,
                        g.likedByUsers,
                        g.fromPoints,
                        voteForPoint = g.article.VoteForPoint,
                        author = g.article.Principal.User,
                        likeCount = g.article.Likes.Count(l => l.Backout == false),
                        commentCount = g.article.Comments.Count,
                        typeName = g.article.Type.Name
                    })
                    .ToListAsync();

                var result = articleEntries.Select(entry =>
                {
                    var articleDTO = new ArticleDTO(entry.article, true, 256, true)
                    {
                        TimelineReason = entry.reason,
                        LikeCount = entry.likeCount,
                        CommentCount = entry.commentCount,
                        TypeName = entry.typeName,
                        Author = new UserDTO(entry.author),
                        VoteForPoint = entry.voteForPoint == null ? null : new NormalPointDTO(entry.voteForPoint, true)
                    };
                    if (string.IsNullOrEmpty(entry.article.ThumbnailImage))
                    {
                        articleDTO.ThumbnailImage = entry.voteForPoint?.BackgroundImage;
                    }
                    if (articleDTO.TypeName != "简评")
                        articleDTO.TruncateContent(128);
                    switch (entry.reason)
                    {
                        case ArticleDTO.TimelineReasonType.Point:
                            if (!entry.fromPoints.Select(p => p.Id).Contains(entry.voteForPoint?.Id))
                                articleDTO.AttachedPoints =
                                    entry.fromPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                            break;

                        case ArticleDTO.TimelineReasonType.Like:
                            articleDTO.LikeByUsers = entry.likedByUsers.Select(u => new UserDTO(u)).ToList();
                            break;
                    }
                    return articleDTO;
                }).ToList();
                if (useCache)
                    await RedisProvider.Set(cacheKey, RedisProvider.Serialize(result), TimeSpan.FromHours(12));
                return result;*/
        }
    }
}