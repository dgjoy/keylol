using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.ActivityComment
{
    /// <summary>
    /// 动态评论 Controller
    /// </summary>
    public partial class ActivityCommentController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="ActivityCommentController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public ActivityCommentController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}