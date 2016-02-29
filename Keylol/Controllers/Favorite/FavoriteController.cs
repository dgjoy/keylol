using System.Web.Http;

namespace Keylol.Controllers.Favorite
{
    [Authorize]
    [RoutePrefix("favorite")]
    public partial class FavoriteController : KeylolApiController
    {
        private const int FavoriteSize = 5;
    }
}