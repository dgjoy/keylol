using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Favorite
{
    /// <summary>
    ///     收藏夹 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("favorite")]
    public partial class FavoriteController : ApiController
    {
        private const int FavoriteSize = 5;
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="FavoriteController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public FavoriteController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}