using System.Web.Http;
using Keylol.Utilities;

namespace Keylol.Controllers.SteamBot
{
    /// <summary>
    /// Steam 机器人 Controller
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("steam-bot")]
    public partial class SteamBotController : KeylolApiController
    {
    }
}