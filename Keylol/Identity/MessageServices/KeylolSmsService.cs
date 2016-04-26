using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity.MessageServices
{
    /// <summary>
    /// SMS service provider for ASP.NET Identity
    /// </summary>
    public class KeylolSmsService : IIdentityMessageService
    {
        /// <summary>
        /// This method should send the message
        /// </summary>
        /// <param name="message"/>
        /// <returns/>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}