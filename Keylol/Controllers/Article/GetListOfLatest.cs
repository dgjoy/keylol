using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取全站最新文章
        /// </summary>
        /// <param name="titleOnly">是否仅获取文章的标题和时间，默认 true</param>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSn">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("latest")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDto>))]
        public async Task<IHttpActionResult> GetListOfLatest(bool titleOnly = true, string articleTypeFilter = null,
            int beforeSn = int.MaxValue, int take = 30)
        {
            if (take > 50) take = 50;
            var articleQuery = DbContext.Articles.AsNoTracking()
                .Where(a => a.Archived == ArchivedState.None && a.Rejected == false && a.SequenceNumber < beforeSn);
            if (articleTypeFilter != null)
            {
                var types = articleTypeFilter.Split(',').Select(s => s.Trim().ToEnum<ArticleType>()).ToList();
                articleQuery =
                    articleQuery.Where(PredicateBuilder.Contains<Models.Article, ArticleType>(types, a => a.Type));
            }
            articleQuery = articleQuery.OrderByDescending(a => a.SequenceNumber).Take(() => take);
            if (titleOnly)
            {
                var articleEntries = await articleQuery.Select(a => new
                {
                    article = a,
                    authorIdCode = a.Principal.User.IdCode
                }).ToListAsync();
                return Ok(articleEntries.Select(e => new ArticleDto
                {
                    Id = e.article.Id,
                    PublishTime = e.article.PublishTime,
                    Title = e.article.Title,
                    AuthorIdCode = e.authorIdCode,
                    SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                }));
            }
            else
            {
                var articleEntries = await articleQuery.Select(a => new
                {
                    article = a,
                    likeCount = a.Likes.Count,
                    commentCount = a.Comments.Count,
                    type = a.Type,
                    author = a.Principal.User,
                    voteForPoint = a.VoteForPoint
                }).ToListAsync();
                return Ok(articleEntries.Select(entry =>
                {
                    var articleDto = new ArticleDto(entry.article, true, 256, true)
                    {
                        LikeCount = entry.likeCount,
                        CommentCount = entry.commentCount,
                        TypeName = entry.type.ToString(),
                        Author = new UserDto(entry.author),
                        VoteForPoint = entry.voteForPoint == null ? null : new NormalPointDto(entry.voteForPoint, true)
                    };
                    if (string.IsNullOrEmpty(entry.article.ThumbnailImage))
                    {
                        articleDto.ThumbnailImage = entry.voteForPoint?.BackgroundImage;
                    }
                    if (entry.type != ArticleType.简评)
                        articleDto.TruncateContent(128);
                    return articleDto;
                }));
            }
        }
    }
}