using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.InvitationCode
{
    /// <summary>
    ///     邀请码 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("invitation-code")]
    public partial class InvitationCodeController : KeylolApiController
    {
    }
}