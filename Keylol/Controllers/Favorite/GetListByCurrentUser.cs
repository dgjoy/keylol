using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Keylol.Provider;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Favorite
{
    public partial class FavoriteController
    {
        /// <summary>
        ///     获取用户收藏夹内容
        /// </summary>
        [Route]
        [HttpGet]
        [ResponseType(typeof (List<FavoriteDTO>))]
        public async Task<IHttpActionResult> GetListByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            var cacheKey = $"user:{userId}:favorites";
            var cache = await RedisProvider.Get(cacheKey);
            if (cache.HasValue)
                return Ok(RedisProvider.Deserialize(cache, true));

            var result = (await DbContext.Favorites.Where(f => f.UserId == userId)
                .OrderBy(f => f.AddTime)
                .Take(() => FavoriteSize)
                .ToListAsync()).Select(f => new FavoriteDTO(f)).ToList();
            await RedisProvider.Set(cacheKey, RedisProvider.Serialize(result), TimeSpan.FromDays(7));
            return Ok(result);
        }
    }
}