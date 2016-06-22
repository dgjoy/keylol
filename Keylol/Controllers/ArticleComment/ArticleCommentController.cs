using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using RabbitMQ.Client;

namespace Keylol.Controllers.ArticleComment
{
    /// <summary>
    ///     评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("article-comment")]
    public partial class ArticleCommentController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CachedDataProvider _cachedData;
        private readonly IModel _mqChannel;

        /// <summary>
        ///     创建 <see cref="ArticleCommentController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="mqChannel"><see cref="IModel"/></param>
        public ArticleCommentController(KeylolDbContext dbContext, KeylolUserManager userManager,
            CachedDataProvider cachedData, IModel mqChannel)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
            _mqChannel = mqChannel;
        }
    }
}