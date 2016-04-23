using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.InvitationCode
{
    /// <summary>
    ///     邀请码 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("invitation-code")]
    public partial class InvitationCodeController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="InvitationCodeController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public InvitationCodeController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}