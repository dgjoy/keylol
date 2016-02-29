using System.Web.Http;

namespace Keylol.Controllers.Like
{
    [Authorize]
    [RoutePrefix("like")]
    public partial class LikeController : KeylolApiController
    {
    }
}