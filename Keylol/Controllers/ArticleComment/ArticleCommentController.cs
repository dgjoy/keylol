using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.ArticleComment
{
    /// <summary>
    ///     评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("comment")]
    public partial class ArticleCommentController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="ArticleCommentController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public ArticleCommentController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}