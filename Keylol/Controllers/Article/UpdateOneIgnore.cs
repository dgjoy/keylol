using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     设置是否忽略指定文章以后的认可或评论提醒
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="ignore">是否忽略</param>
        /// <param name="type">要忽略的类型</param>
        [Route("{id}/ignore")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该文章进行操作")]
        public async Task<IHttpActionResult> UpdateOneIgnore(string id, bool ignore, IgnoreType type)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (userId != article.PrincipalId)
                return Unauthorized();

            switch (type)
            {
                case IgnoreType.Like:
                    article.IgnoreNewLikes = ignore;
                    break;

                case IgnoreType.Comment:
                    article.IgnoreNewComments = ignore;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    /// <summary>
    ///     忽略类型
    /// </summary>
    public enum IgnoreType
    {
        /// <summary>
        ///     忽略新认可
        /// </summary>
        Like,

        /// <summary>
        ///     忽略新评论
        /// </summary>
        Comment
    }
}