using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using RabbitMQ.Client;

namespace Keylol.Controllers.Activity
{
    /// <summary>
    /// 动态 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("activity")]
    public partial class ActivityController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly IModel _mqChannel;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="ActivityController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="mqChannel"><see cref="IModel"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public ActivityController(KeylolDbContext dbContext, IModel mqChannel, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _mqChannel = mqChannel;
            _userManager = userManager;
        }
    }
}