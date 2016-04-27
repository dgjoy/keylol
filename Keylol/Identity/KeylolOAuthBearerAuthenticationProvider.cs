using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;

namespace Keylol.Identity
{
    /// <summary>
    ///     OAuth bearer authentication provider Keylol implementation
    /// </summary>
    public class KeylolOAuthBearerAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        /// <summary>
        ///     创建 <see cref="KeylolOAuthBearerAuthenticationProvider" />
        /// </summary>
        public KeylolOAuthBearerAuthenticationProvider()
        {
            OnValidateIdentity = async context =>
            {
                var userManager = Startup.Container.GetInstance<KeylolUserManager>();
                if (userManager.SupportsUserSecurityStamp)
                {
                    // 检查 Security Stamp
                    var userId = context.Ticket.Identity.GetUserId();
                    var securityStamp = context.Ticket.Identity.FindFirstValue(KeylolClaimTypes.SecurityStamp);
                    if (securityStamp != await userManager.GetSecurityStampAsync(userId))
                    {
                        context.Rejected();
                    }
                }
            };
        }
    }
}