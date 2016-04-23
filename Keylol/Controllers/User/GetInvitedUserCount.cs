using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     获取当前登录用户邀请注册的用户数量
        /// </summary>
        [Route("invited-user-count")]
        [HttpGet]
        [ResponseType(typeof (int))]
        public async Task<IHttpActionResult> GetInvitedUserCount()
        {
            var userId = User.Identity.GetUserId();
            return Ok(await _dbContext.Users.CountAsync(u => u.InviterId == userId));
        }
    }
}