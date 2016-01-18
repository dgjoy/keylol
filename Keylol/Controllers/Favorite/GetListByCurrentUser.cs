using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
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
            return Ok((await DbContext.Favorites.Where(f => f.UserId == userId)
                .OrderBy(f => f.AddTime)
                .Take(() => FavoriteSize)
                .ToListAsync()).Select(f => new FavoriteDTO(f)));
        }
    }
}