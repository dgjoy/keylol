using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Controllers.Article;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        ///     设置是否忽略指定评论以后的认可或评论提醒
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="ignore">是否忽略</param>
        /// <param name="type">要忽略的类型</param>
        [Route("{id}/ignore")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该评论进行操作")]
        public async Task<IHttpActionResult> UpdateOneIgnore(string id, bool ignore, IgnoreType type)
        {
            var comment = await DbContext.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (userId != comment.CommentatorId)
                return Unauthorized();

            switch (type)
            {
                case IgnoreType.Like:
                    comment.IgnoreNewLikes = ignore;
                    break;

                case IgnoreType.Comment:
                    comment.IgnoreNewComments = ignore;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}