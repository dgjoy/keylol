using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using System.Collections.Generic;
using Keylol.DAL;
using Microsoft.AspNet.Identity.Owin;

namespace Keylol
{
    public abstract class KeylolApiController : ApiController
    {
        protected KeylolApiController()
        {
        }

        public IOwinContext OwinContext => ((HttpContextWrapper) Request.Properties["MS_HttpContext"]).GetOwinContext();

        public KeylolSignInManager SignInManager => OwinContext.Get<KeylolSignInManager>();

        public KeylolUserManager UserManager => OwinContext.GetUserManager<KeylolUserManager>();

        public KeylolDbContext DbContext => OwinContext.Get<KeylolDbContext>();
    }
}