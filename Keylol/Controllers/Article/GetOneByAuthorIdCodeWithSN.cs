using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     根据作者和文章序号取得一篇文章（被封存的文章只能作者和运维职员可见）
        /// </summary>
        /// <param name="authorIdCode">作者 IdCode</param>
        /// <param name="sequenceNumberForAuthor">文章序号</param>
        [Route("{authorIdCode}/{sequenceNumberForAuthor}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (ArticleDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "文章被封存，当前登录用户无权查看")]
        public async Task<IHttpActionResult> GetOneByAuthorIdCodeWithSn(string authorIdCode, int sequenceNumberForAuthor)
        {
            var userId = User.Identity.GetUserId();
            var articleEntry =
                await
                    _dbContext.Articles.Where(a =>
                        a.Principal.User.IdCode == authorIdCode &&
                        a.SequenceNumberForAuthor == sequenceNumberForAuthor)
                        .Select(a => new
                        {
                            article = a,
                            likeCount = a.Likes.Count(),
                            liked = a.Likes.Any(l => l.OperatorId == userId),
                            type = a.Type,
                            attachedPoints = a.AttachedPoints,
                            voteForPoint = a.VoteForPoint
                        })
                        .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();

            var staffClaim = string.IsNullOrEmpty(userId) ? null : await _userManager.GetStaffClaimAsync(userId);
            if (articleEntry.article.Archived != ArchivedState.None &&
                userId != articleEntry.article.PrincipalId && staffClaim != StaffClaim.Operator)
                return Unauthorized();

            var articleDto = new ArticleDto(articleEntry.article, true, includeProsCons: true, includeSummary: true)
            {
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDto(point, true)).ToList(),
                TypeName = articleEntry.type.ToString(),
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked,
                Archived = articleEntry.article.Archived,
                Rejected = articleEntry.article.Rejected,
                Spotlight = articleEntry.article.SpotlightTime != null,
                Warned = articleEntry.article.Warned
            };
            if (articleEntry.voteForPoint != null)
                articleDto.VoteForPoint = new NormalPointDto(articleEntry.voteForPoint, true);
            return Ok(articleDto);
        }
    }
}