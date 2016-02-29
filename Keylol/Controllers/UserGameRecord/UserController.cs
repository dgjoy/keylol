using System.Web.Http;

namespace Keylol.Controllers.UserGameRecord
{
    [Authorize]
    [RoutePrefix("user-game-record")]
    public partial class UserGameRecordController : KeylolApiController
    {
    }
}