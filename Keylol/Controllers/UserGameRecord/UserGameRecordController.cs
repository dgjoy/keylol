using System.Web.Http;
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

        /// <summary>
        /// 创建 <see cref="UserGameRecordController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public UserGameRecordController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}