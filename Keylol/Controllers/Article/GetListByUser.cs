using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Controllers.User;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Provider;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取指定用户时间轴的文章
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="source">来源过滤器，1 表示发表的文章，2 表示认可的文章，默认 1</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("user/{userId}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetListByUser(string userId, UserController.IdType idType,
            string articleTypeFilter = null, int source = 1, int beforeSN = int.MaxValue, int take = 30)
        {
            var redisDb = RedisProvider.GetInstance().GetDatabase();
            KeylolUser user;
            switch (idType)
            {
                case UserController.IdType.Id:
                    user = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == userId);
                    break;

                case UserController.IdType.IdCode:
                    user = await DbContext.Users.AsNoTracking().SingleAsync(u => u.IdCode == userId);
                    break;

                case UserController.IdType.UserName:
                    user = await DbContext.Users.AsNoTracking().SingleAsync(u => u.UserName == userId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }
            var cacheKey = $"user:{user.Id}:profile.timeline";
            var useCache = articleTypeFilter == null && source == 3 && beforeSN == int.MaxValue;
            if (useCache)
            {
                var cache = await redisDb.StringGetAsync(cacheKey);
                if (cache.HasValue)
                    return Ok(RedisProvider.Deserialize(cache, true));
            }

            if (take > 50) take = 50;
            var userQuery = DbContext.Users.AsNoTracking().Where(u => u.Id == user.Id);
            var publishedQuery = userQuery.SelectMany(u => u.ProfilePoint.Entries.OfType<Models.Article>())
                .Where(a => a.SequenceNumber < beforeSN)
                .Select(a => new
                {
                    article = a,
                    reason = ArticleDTO.TimelineReasonType.Publish,
                    author = (KeylolUser) null
                });
            var likedQuery = userQuery.SelectMany(u => u.Likes.OfType<ArticleLike>())
                .Where(l => l.Backout == false && l.Article.SequenceNumber < beforeSN)
                .Select(l => new
                {
                    article = l.Article,
                    reason = ArticleDTO.TimelineReasonType.Like,
                    author = l.Article.Principal.User
                });
            var published = (source & 1) != 0;
            var liked = (source & 1 << 1) != 0;
            var articleQuery = publishedQuery;
            if (published)
            {
                if (liked)
                    articleQuery = articleQuery.Concat(likedQuery);
            }
            else
            {
                if (liked)
                    articleQuery = likedQuery;
                else
                    return Ok();
            }
            if (articleTypeFilter != null)
            {
                var typesName = articleTypeFilter.Split(',').Select(s => s.Trim()).ToList();
                articleQuery = articleQuery.Where(PredicateBuilder.Contains(typesName, a => a.article.Type.Name, new
                {
                    article = (Models.Article) null,
                    reason = ArticleDTO.TimelineReasonType.Like,
                    author = (KeylolUser) null
                }));
            }
            var articleEntries = await articleQuery.GroupBy(e => e.article)
                .OrderByDescending(g => g.Key.SequenceNumber).Take(() => take)
                .Select(g => new
                {
                    article = g.Key,
                    candicates = g,
                    reason = g.Max(ee => ee.reason)
                })
                .Select(g => new
                {
                    g.article,
                    g.reason,
                    g.candicates.FirstOrDefault(e => e.reason == g.reason).author,
                    voteForPoint = g.article.VoteForPoint,
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
                    VoteForPoint = entry.voteForPoint == null ? null : new NormalPointDTO(entry.voteForPoint, true)
                };
                if (string.IsNullOrEmpty(entry.article.ThumbnailImage))
                {
                    articleDTO.ThumbnailImage = entry.voteForPoint?.BackgroundImage;
                }
                if (articleDTO.TypeName != "简评")
                    articleDTO.TruncateContent(128);
                if (entry.reason != ArticleDTO.TimelineReasonType.Publish)
                {
                    articleDTO.Author = new UserDTO(entry.author);
                }
                return articleDTO;
            }).ToList();
            if (useCache)
                await redisDb.StringSetAsync(cacheKey, RedisProvider.Serialize(result), TimeSpan.FromDays(7));
            return Ok(result);
        }
    }
}