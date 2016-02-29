using System.Web.Http;

namespace Keylol.Controllers.UserPointSubscription
{
    [Authorize]
    [RoutePrefix("user-point-subscription")]
    public partial class UserPointSubscriptionController : KeylolApiController
    {
    }
}