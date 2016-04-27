using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Utilities;
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
            var count = await _dbContext.Favorites.Where(f => f.UserId == userId).CountAsync();

            if (count >= FavoriteSize)
                return this.BadRequest(nameof(pointId), Errors.TooMany);

            var favorite = _dbContext.Favorites.Create();
            favorite.UserId = userId;
            favorite.PointId = pointId;
            _dbContext.Favorites.Add(favorite);
            await _dbContext.SaveChangesAsync();
            return Created($"favorite/{favorite.Id}", favorite.Id);
        }
    }
}