using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        ///     设置是否忽略指定评论以后的认可提醒
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="ignore">是否忽略</param>
        [Route("{id}/ignore")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该评论进行操作")]
        public async Task<IHttpActionResult> UpdateOneIgnoreNewLikes(string id, bool ignore)
        {
            var comment = await DbContext.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (userId != comment.CommentatorId)
                return Unauthorized();

            comment.IgnoreNewLikes = ignore;
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}