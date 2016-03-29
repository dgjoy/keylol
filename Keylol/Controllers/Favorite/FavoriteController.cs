using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Favorite
{
    /// <summary>
    ///     收藏夹 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("favorite")]
    public partial class FavoriteController : KeylolApiController
    {
        private const int FavoriteSize = 5;
    }
}