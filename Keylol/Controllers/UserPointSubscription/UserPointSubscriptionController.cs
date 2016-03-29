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
        /// <summary>
        /// 创建 UserPointSubscriptionController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public UserPointSubscriptionController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}