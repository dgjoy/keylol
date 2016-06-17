using System.Web.Http;
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

        /// <summary>
        /// 创建 <see cref="ActivityController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="mqChannel"><see cref="IModel"/></param>
        public ActivityController(KeylolDbContext dbContext, IModel mqChannel)
        {
            _dbContext = dbContext;
            _mqChannel = mqChannel;
        }
    }
}