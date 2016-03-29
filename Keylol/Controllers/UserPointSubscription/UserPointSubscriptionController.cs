using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.UserPointSubscription
{
    /// <summary>
    ///     据点订阅 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-point-subscription")]
    public partial class UserPointSubscriptionController : KeylolApiController
    {
    }
}