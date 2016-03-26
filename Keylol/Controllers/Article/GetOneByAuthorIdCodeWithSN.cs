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
        [ResponseType(typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "文章被封存，当前登录用户无权查看")]
        public async Task<IHttpActionResult> GetOneByAuthorIdCodeWithSN(string authorIdCode, int sequenceNumberForAuthor)
        {
            var userId = User.Identity.GetUserId();
            var articleEntry =
                await
                    DbContext.Articles.Where(a =>
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
            var staffClaim = await UserManager.GetStaffClaimAsync(userId);
            if (articleEntry.article.Archived != ArchivedState.None &&
                userId != articleEntry.article.PrincipalId && staffClaim != StaffClaim.Operator)
                return Unauthorized();
            var articleDto = new ArticleDTO(articleEntry.article, true, includeProsCons: true, includeSummary: true)
            {
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.type.ToString(),
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked,
                Archived = articleEntry.article.Archived,
                Rejected = articleEntry.article.Rejected,
                Spotlight = articleEntry.article.SpotlightTime != null,
                Warned = articleEntry.article.Warned
            };
            if (articleEntry.voteForPoint != null)
                articleDto.VoteForPoint = new NormalPointDTO(articleEntry.voteForPoint, true);
            return Ok(articleDto);
        }
    }
}