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
        ///     设置是否忽略指定文章以后的认可提醒
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="ignore">是否忽略</param>
        [Route("{id}/ignore")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该文章进行操作")]
        public async Task<IHttpActionResult> UpdateOneIgnoreNewLikes(string id, bool ignore)
        {
            var article = await DbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (userId != article.PrincipalId)
                return Unauthorized();

            article.IgnoreNewLikes = ignore;
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}