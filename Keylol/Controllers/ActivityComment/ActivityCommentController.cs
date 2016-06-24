using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.Controllers.ActivityComment
{
    /// <summary>
    /// 动态评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("activity-comment")]
    public partial class ActivityCommentController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly CachedDataProvider _cachedData;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="ActivityCommentController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public ActivityCommentController(KeylolDbContext dbContext, CachedDataProvider cachedData,
            KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _cachedData = cachedData;
            _userManager = userManager;
        }
    }
}