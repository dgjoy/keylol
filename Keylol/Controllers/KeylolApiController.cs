using System.Net.Http;
using System.Web.Http;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Keylol.Controllers
{
    /// <summary>
    /// API Controller 可以继承自这个类，获得一些方便调用的属性
    /// </summary>
    public abstract class KeylolApiController : ApiController
    {
        /// <summary>
        /// 当前请求的 OWIN Context
        /// </summary>
        public IOwinContext OwinContext => Request.GetOwinContext();

        /// <summary>
        /// ASP.NET Identity SignInManager
        /// </summary>
        public KeylolSignInManager SignInManager => OwinContext.Get<KeylolSignInManager>();

        /// <summary>
        /// ASP.NET Identity UserManager
        /// </summary>
        public KeylolUserManager UserManager => OwinContext.GetUserManager<KeylolUserManager>();

        /// <summary>
        /// OWIN Context Authentication 对象（IAuthenticationManager）
        /// </summary>
        public IAuthenticationManager AuthenticationManager => OwinContext.Authentication;

        /// <summary>
        /// Keylol DbContext
        /// </summary>
        public KeylolDbContext DbContext => OwinContext.Get<KeylolDbContext>();
    }
}