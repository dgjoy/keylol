using System.Web.Http;

namespace Keylol.Controllers.InvitationCode
{
    [Authorize]
    [RoutePrefix("invitation-code")]
    public partial class InvitationCodeController : KeylolApiController
    {
    }
}