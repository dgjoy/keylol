using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.Controllers.Subscription
{
    /// <summary>
    /// 订阅 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("subscription")]
    public partial class SubscriptionController : ApiController
    {
        private readonly CachedDataProvider _cachedData;
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="SubscriptionController"/>
        /// </summary>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public SubscriptionController(CachedDataProvider cachedData, KeylolDbContext dbContext,
            KeylolUserManager userManager)
        {
            _cachedData = cachedData;
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}