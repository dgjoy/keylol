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
        ///     根据 ID 取得一篇文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        [Route("{id}")]
        [HttpGet]
        [ResponseType(typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        public async Task<IHttpActionResult> GetOneById(string id)
        {
            var userId = User.Identity.GetUserId();
            var articleEntry = await DbContext.Articles.Where(a => a.Id == id).Select(
                a =>
                    new
                    {
                        article = a,
                        likeCount = a.Likes.Count(l => l.Backout == false),
                        liked = a.Likes.Any(l => l.OperatorId == userId && l.Backout == false),
                        typeName = a.Type.Name,
                        attachedPoints = a.AttachedPoints,
                        authorIdCode = a.Principal.User.IdCode,
                        voteForPoint = a.VoteForPoint
                    })
                .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article, true)
            {
                AuthorIdCode = articleEntry.authorIdCode,
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.typeName,
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            if (articleEntry.voteForPoint != null)
                articleDTO.VoteForPoint = new NormalPointDTO(articleEntry.voteForPoint, true);
            return Ok(articleDTO);
        }
    }
}