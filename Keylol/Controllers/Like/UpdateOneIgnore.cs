using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     设置是否在提醒中忽略指定认可
        /// </summary>
        /// <param name="id">认可 ID</param>
        /// <param name="ignore">是否忽略</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定认可不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该认可进行操作")]
        public async Task<IHttpActionResult> UpdateOneIgnore(string id, bool ignore)
        {
            var like = await DbContext.Likes.FindAsync(id);
            if (like == null)
                return NotFound();

            string targetUserId = null;
            var articleLike = like as ArticleLike;
            if (articleLike != null)
            {
                targetUserId = articleLike.Article.PrincipalId;
            }
            else
            {
                var commentLike = like as CommentLike;
                if (commentLike != null)
                    targetUserId = commentLike.Comment.CommentatorId;
            }

            var userId = User.Identity.GetUserId();
            if (targetUserId != userId)
                return Unauthorized();

            like.IgnoredByTargetUser = ignore;
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}