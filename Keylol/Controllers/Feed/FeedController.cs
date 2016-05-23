using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Feed
{
    /// <summary>
    /// Feed Controller
    /// </summary>
    [RoutePrefix("feed")]
    [Authorize(Roles = KeylolRoles.Operator)]
    public partial class FeedController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="FeedController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public FeedController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}