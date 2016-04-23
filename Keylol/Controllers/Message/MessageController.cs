using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Message
{
    /// <summary>
    ///     消息 Controller（邮政中心）
    /// </summary>
    [Authorize]
    [RoutePrefix("message")]
    public partial class MessageController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="MessageController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public MessageController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}