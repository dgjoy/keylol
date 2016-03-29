using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.UserGameRecord
{
    /// <summary>
    ///     用户游戏记录 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user-game-record")]
    public partial class UserGameRecordController : KeylolApiController
    {
        /// <summary>
        /// 创建 UserGameRecordController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public UserGameRecordController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}