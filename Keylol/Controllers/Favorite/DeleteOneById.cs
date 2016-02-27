using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
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
        public async Task<IHttpActionResult> DeleteOneById(string id)
        {
            var favorite = await DbContext.Favorites.FindAsync(id);
            if (favorite == null)
                return NotFound();
            DbContext.Favorites.Remove(favorite);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}