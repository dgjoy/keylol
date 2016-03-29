using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.InvitationCode
{
    /// <summary>
    ///     邀请码 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("invitation-code")]
    public partial class InvitationCodeController : KeylolApiController
    {
        /// <summary>
        /// 创建 InvitationCodeController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public InvitationCodeController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}