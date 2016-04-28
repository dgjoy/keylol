using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取当前用户主订阅时间轴的文章
        /// </summary>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="shortReviewFilter">简评来源过滤器，1 表示关注用户和认可，2 表示手动订阅据点，4 表示同步订阅列表，默认 1</param>
        /// <param name="beforeSn">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("subscription")]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDto>))]
        public async Task<IHttpActionResult> GetBySubscription(string articleTypeFilter = null,
            int shortReviewFilter = 1, int beforeSn = int.MaxValue, int take = 30)
        {
            var userId = User.Identity.GetUserId();

            if (take > 50) take = 50;
            var userQuery = _dbContext.Users.AsNoTracking().Where(u => u.Id == userId);
            var profilePointsQuery = userQuery.SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>());

            var shortReviewFilter1 = (shortReviewFilter & 1) != 0;
            var shortReviewFilter2 = (shortReviewFilter & (1 << 1)) != 0;
            var shortReviewFilter3 = (shortReviewFilter & (1 << 2)) != 0;
            var articleQuery =
                userQuery.SelectMany(u => u.SubscribedPoints.OfType<Models.NormalPoint>())
                    .SelectMany(p => p.Articles.Select(a => new {article = a, fromPoint = p}))
                    .Where(e => e.article.SequenceNumber < beforeSn &&
                                e.article.Archived == ArchivedState.None && e.article.Rejected == false &&
                                (shortReviewFilter2 || e.article.Type != ArticleType.简评))
                    .Select(e => new
                    {
                        e.article,
                        e.fromPoint,
                        reason = ArticleDto.TimelineReasonType.Point,
                        likedByUser = (KeylolUser) null
                    })
                    .Concat(profilePointsQuery.SelectMany(p => p.Articles)
                        .Where(a => a.SequenceNumber < beforeSn &&
                                    a.Archived == ArchivedState.None && a.Rejected == false &&
                                    (shortReviewFilter1 || a.Type != ArticleType.简评))
                        .Select(a => new
                        {
                            article = a,
                            fromPoint = (Models.NormalPoint) null,
                            reason = ArticleDto.TimelineReasonType.Publish,
                            likedByUser = (KeylolUser) null
                        }))
                    .Concat(profilePointsQuery.Select(p => p.User)
                        .SelectMany(u => u.Likes.OfType<ArticleLike>())
                        .Where(l => l.Article.SequenceNumber < beforeSn &&
                                    l.Article.Archived == ArchivedState.None && l.Article.Rejected == false &&
                                    (shortReviewFilter1 || l.Article.Type != ArticleType.简评))
                        .Select(l => new
                        {
                            article = l.Article,
                            fromPoint = (Models.NormalPoint) null,
                            reason = ArticleDto.TimelineReasonType.Like,
                            likedByUser = l.Operator
                        }))
                    .Concat(_dbContext.AutoSubscriptions.Where(s => s.UserId == userId)
                        .SelectMany(
                            s => s.NormalPoint.Articles.Select(a => new {article = a, fromPoint = s.NormalPoint}))
                        .Where(e =>
                            e.article.SequenceNumber < beforeSn &&
                            e.article.Archived == ArchivedState.None && e.article.Rejected == false &&
                            (shortReviewFilter3 || e.article.Type != ArticleType.简评))
                        .Select(e => new
                        {
                            e.article,
                            e.fromPoint,
                            reason = ArticleDto.TimelineReasonType.Point,
                            likedByUser = (KeylolUser) null
                        }));

            if (articleTypeFilter != null)
            {
                var types = articleTypeFilter.Split(',').Select(s => s.Trim().ToEnum<ArticleType>()).ToList();
                if (shortReviewFilter != 0)
                    types.Add(ArticleType.简评);
                articleQuery = articleQuery.Where(PredicateBuilder.Contains(types, a => a.article.Type, new
                {
                    article = (Models.Article) null,
                    fromPoint = (Models.NormalPoint) null,
                    reason = ArticleDto.TimelineReasonType.Like,
                    likedByUser = (KeylolUser) null
                }));
            }

            var articleEntries = await articleQuery.GroupBy(e => e.article)
                .OrderByDescending(g => g.Key.SequenceNumber).Take(() => take)
                .Select(g => new
                {
                    article = g.Key,
                    likedByUsers = g.Where(e => e.reason == ArticleDto.TimelineReasonType.Like)
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
                    likeCount = g.article.Likes.Count,
                    commentCount = g.article.Comments.Count,
                    type = g.article.Type
                })
                .ToListAsync();

            return Ok(articleEntries.Select(entry =>
            {
                var articleDto = new ArticleDto(entry.article, true, 256, true)
                {
                    TimelineReason = entry.reason,
                    LikeCount = entry.likeCount,
                    CommentCount = entry.commentCount,
                    TypeName = entry.type.ToString(),
                    Author = new UserDto(entry.author),
                    VoteForPoint = entry.voteForPoint == null ? null : new NormalPointDto(entry.voteForPoint, true)
                };
                if (string.IsNullOrWhiteSpace(entry.article.ThumbnailImage))
                {
                    articleDto.ThumbnailImage = entry.voteForPoint?.BackgroundImage;
                }
                if (entry.type != ArticleType.简评)
                    articleDto.TruncateContent(128);
                switch (entry.reason)
                {
                    case ArticleDto.TimelineReasonType.Point:
                        if (!entry.fromPoints.Select(p => p.Id).Contains(entry.voteForPoint?.Id))
                            articleDto.AttachedPoints =
                                entry.fromPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        break;

                    case ArticleDto.TimelineReasonType.Like:
                        articleDto.LikeByUsers = entry.likedByUsers.Select(u => new UserDto(u)).ToList();
                        break;
                }
                return articleDto;
            }).ToList());
        }
    }
}