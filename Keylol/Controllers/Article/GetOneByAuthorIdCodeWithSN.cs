using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     根据作者和文章序号取得一篇文章
        /// </summary>
        /// <param name="authorIdCode">作者 IdCode</param>
        /// <param name="sequenceNumberForAuthor">文章序号</param>
        [Route("{authorIdCode}/{sequenceNumberForAuthor}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
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
                            likeCount = a.Likes.Count(l => l.Backout == false),
                            liked = a.Likes.Any(l => l.OperatorId == userId && l.Backout == false),
                            type = a.Type,
                            attachedPoints = a.AttachedPoints,
                            voteForPoint = a.VoteForPoint
                        })
                        .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article, true, includeProsCons: true, includeSummary: true)
            {
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.type.ToString(),
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            if (articleEntry.voteForPoint != null)
                articleDTO.VoteForPoint = new NormalPointDTO(articleEntry.voteForPoint, true);
            return Ok(articleDTO);
        }
    }
}