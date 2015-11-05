using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("favorite")]
    public class FavoriteController : KeylolApiController
    {
        private const int FavoriteSize = 5;

        /// <summary>
        /// 获取用户收藏夹内容
        /// </summary>
        [Route]
        [ResponseType(typeof (List<FavoriteDTO>))]
        public async Task<IHttpActionResult> Get()
        {
            var userId = User.Identity.GetUserId();
            return Ok((await DbContext.Favorites.Where(f => f.UserId == userId)
                .OrderBy(f => f.AddTime)
                .Take(() => FavoriteSize)
                .ToListAsync()).Select(f => new FavoriteDTO(f)));
        }

        /// <summary>
        /// 创建一个新收藏
        /// </summary>
        /// <param name="pointId">据点 ID 或用户 ID</param>
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(string pointId)
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
            return Created($"favorite/{favorite.Id}", favorite.Id);
        }

        /// <summary>
        /// 删除一个收藏
        /// </summary>
        /// <param name="id">收藏 ID</param>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定收藏不存在")]
        public async Task<IHttpActionResult> Delete(string id)
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