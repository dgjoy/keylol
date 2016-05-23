using System.Web.Http;
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

        /// <summary>
        /// 创建 <see cref="SubscriptionController"/>
        /// </summary>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public SubscriptionController(CachedDataProvider cachedData, KeylolDbContext dbContext)
        {
            _cachedData = cachedData;
            _dbContext = dbContext;
        }
    }
}