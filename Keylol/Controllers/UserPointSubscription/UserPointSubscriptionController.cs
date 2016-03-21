using System.Web.Http;

namespace Keylol.Controllers.UserPointSubscription
{
    /// <summary>
    /// 据点订阅 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-point-subscription")]
    public partial class UserPointSubscriptionController : KeylolApiController
    {
    }
}