using System.Web.Http;

namespace Keylol.Controllers.Login
{
    public partial class LoginController
    {
        /// <summary>
        ///     登出当前用户（清除 Cookies）
        /// </summary>
        [Route("current")]
        [HttpDelete]
        public IHttpActionResult DeleteOneByCurrentUser()
        {
            AuthenticationManager.SignOut();
            return Ok();
        }
    }
}