using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.UserPointSubscription
{
    /// <summary>
    ///     据点订阅 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-point-subscription")]
    public partial class UserPointSubscriptionController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="UserPointSubscriptionController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public UserPointSubscriptionController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}