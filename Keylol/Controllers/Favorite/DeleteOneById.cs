using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Favorite
{
    public partial class FavoriteController
    {
        /// <summary>
        ///     删除一个收藏
        /// </summary>
        /// <param name="id">收藏 ID</param>
        [Route("{id}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定收藏不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户不是这个收藏的拥有者")]
        public async Task<IHttpActionResult> DeleteOneById(string id)
        {
            var favorite = await _dbContext.Favorites.FindAsync(id);
            if (favorite == null)
                return NotFound();

            if (favorite.UserId != User.Identity.GetUserId())
                return Unauthorized();

            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}