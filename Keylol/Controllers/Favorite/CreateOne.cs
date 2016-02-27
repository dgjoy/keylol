using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Provider;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Favorite
{
    public partial class FavoriteController
    {
        /// <summary>
        ///     创建一个新收藏
        /// </summary>
        /// <param name="pointId">据点 ID 或用户 ID</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOne(string pointId)
        {
            var userId = User.Identity.GetUserId();
            var count = await DbContext.Favorites.Where(f => f.UserId == userId).CountAsync();
            if (count >= FavoriteSize)
            {
                ModelState.AddModelError("pointId", "收藏夹已满。");
                return BadRequest(ModelState);
            }
            var favorite = DbContext.Favorites.Create();
            favorite.UserId = userId;
            favorite.PointId = pointId;
            DbContext.Favorites.Add(favorite);
            await DbContext.SaveChangesAsync();
            await RedisProvider.Delete($"user:{userId}:favorites");
            return Created($"favorite/{favorite.Id}", favorite.Id);
        }
    }
}