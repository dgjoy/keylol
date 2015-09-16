using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace Keylol.Provider
{
    public class KeylolOAuthProvider : OAuthAuthorizationServerProvider
    {
        private const string ClientAngularApp = "angular-app";

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                switch (clientId)
                {
                    case ClientAngularApp:
                        context.Validated(clientId);
                        break;
                }
            }
            return Task.FromResult(0);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return Task.FromResult(0);
        }
    }
}