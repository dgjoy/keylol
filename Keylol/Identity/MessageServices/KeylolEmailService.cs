using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity.MessageServices
{
    /// <summary>
    ///     Email service provider for ASP.NET Identity
    /// </summary>
    public class KeylolEmailService : IIdentityMessageService
    {
        /// <summary>
        ///     This method should send the message
        /// </summary>
        /// <param name="message" />
        /// <returns />
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }
}