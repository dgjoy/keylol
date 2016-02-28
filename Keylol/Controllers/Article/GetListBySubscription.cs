using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
using Newtonsoft.Json;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取当前用户主订阅时间轴的文章
        /// </summary>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("subscription")]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetBySubscription(string articleTypeFilter = null,
            int beforeSN = int.MaxValue, int take = 30)
        {
            var userId = User.Identity.GetUserId();
            var cacheKey = $"user:{userId}:subscription.timeline";

            Func<KeylolDbContext, Task<IEnumerable<ArticleDTO>>> calculate = async (dbContext) =>
            {
                if (take > 50) take = 50;
                var userQuery = dbContext.Users.AsNoTracking().Where(u => u.Id == userId);
                var profilePointsQuery = userQuery.SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>());

                var articleQuery =
                    userQuery.SelectMany(u => u.SubscribedPoints.OfType<Models.NormalPoint>())
                        .SelectMany(p => p.Articles.Select(a => new {article = a, fromPoint = p}))
                        .Where(e => e.article.SequenceNumber < beforeSN)
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
                        .Concat(dbContext.AutoSubscriptionses.Where(s => s.UserId == userId)
                            .SelectMany(
                                s => s.NormalPoint.Articles.Select(a => new {article = a, fromPoint = s.NormalPoint}))
                            .Where(e => e.article.SequenceNumber < beforeSN)
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
                if (beforeSN == int.MaxValue)
                    await RedisProvider.Set(cacheKey, RedisProvider.Serialize(result), TimeSpan.FromDays(7));
                return result;
            };

            if (articleTypeFilter != null || beforeSN != int.MaxValue)
                return Ok(await calculate(DbContext));

            var cache = await RedisProvider.Get(cacheKey);
            if (cache.IsNullOrEmpty)
                return Ok(await calculate(DbContext));

            RedisProvider.BackgroundJob(async () =>
            {
                using (var dbContext = KeylolDbContext.Create())
                {
                    await calculate(dbContext);
                }
            });
            return Ok(RedisProvider.Deserialize(cache, true));
        }
    }
}