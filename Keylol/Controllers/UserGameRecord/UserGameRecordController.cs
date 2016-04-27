using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.UserGameRecord
{
    /// <summary>
    ///     用户游戏记录 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-game-record")]
    public partial class UserGameRecordController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="UserGameRecordController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public UserGameRecordController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}