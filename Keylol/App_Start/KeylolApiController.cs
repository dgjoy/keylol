using System.Net.Http;
using System.Web.Http;
using Keylol.DAL;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Keylol
{
    public abstract class KeylolApiController : ApiController
    {
        public IOwinContext OwinContext => Request.GetOwinContext();
        public KeylolSignInManager SignInManager => OwinContext.Get<KeylolSignInManager>();
        public KeylolUserManager UserManager => OwinContext.GetUserManager<KeylolUserManager>();
        public IAuthenticationManager AuthenticationManager => OwinContext.Authentication;
        public KeylolDbContext DbContext => OwinContext.Get<KeylolDbContext>();
    }
}