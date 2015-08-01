using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Keylol.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Keylol.Provider
{
    public class KeylolOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                if (clientSecret == clientId + "haha")
                    context.Validated(clientId);
            }
        }

        public override async Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<KeylolUserManager>();
            var user = await userManager.FindByNameAsync("stackia");
            context.Validated(await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType));
        }
    }
}