using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using RestSharp;
using RestSharp.Authenticators;

namespace Keylol.Identity.MessageServices
{
    /// <summary>
    ///     Email service provider for ASP.NET Identity
    /// </summary>
    public class KeylolEmailService : IIdentityMessageService
    {
        /// <summary>
        /// 全局默认实例
        /// </summary>
        public static KeylolEmailService Default = new KeylolEmailService();

        private readonly RestClient _restClient = new RestClient("http://api.sendcloud.net/apiv2")
        {
            Authenticator =
                new SimpleAuthenticator("apiUser", ConfigurationManager.AppSettings["sendCloudApiUser"], "apiKey",
                    ConfigurationManager.AppSettings["sendCloudApiKey"])
        };

        private KeylolEmailService()
        {
        }

        /// <summary>
        ///     This method should send the message
        /// </summary>
        /// <param name="message" />
        /// <returns />
        public async Task SendAsync(IdentityMessage message)
        {
            var request = new RestRequest {Resource = "mail/send"};
            request.AddParameter("from", "Keylol Postman <postman@noreply.keylol.com>");
            request.AddParameter("to", message.Destination);
            request.AddParameter("subject", message.Subject);
            request.AddParameter("html", message.Body);
            await _restClient.ExecutePostTaskAsync(request);
        }
    }
}