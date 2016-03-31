using System.Web.Http;

namespace Keylol.Controllers.UserGameRecord
{
    /// <summary>
    ///     用户游戏记录 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-game-record")]
    public partial class UserGameRecordController : KeylolApiController
    {
    }
}