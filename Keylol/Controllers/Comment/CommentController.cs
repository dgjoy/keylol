using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Comment
{
    /// <summary>
    ///     评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("comment")]
    public partial class CommentController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="CommentController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public CommentController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}