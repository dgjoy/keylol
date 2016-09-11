using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using RestSharp;
using RestSharp.Authenticators;

namespace Keylol.Identity.MessageServices
{
    /// <summary>
    ///     SMS service provider for ASP.NET Identity
    /// </summary>
    public class KeylolSmsService : IIdentityMessageService
    {
        /// <summary>
        /// 全局默认实例
        /// </summary>
        public static KeylolSmsService Default = new KeylolSmsService();

        private readonly RestClient _restClient = new RestClient("https://sms.yunpian.com/v2");
        private readonly string _apiKey = ConfigurationManager.AppSettings["yunpianApiKey"] ?? string.Empty;

        private KeylolSmsService()
        {
        }

        /// <summary>
        ///     This method should send the message
        /// </summary>
        /// <param name="message" />
        /// <returns />
        public async Task SendAsync(IdentityMessage message)
        {
            var request = new RestRequest {Resource = "sms/batch_send.json" };
            request.AddParameter("apikey", _apiKey);
            request.AddParameter("mobile", message.Destination);
            request.AddParameter("text", message.Body);
            await _restClient.ExecutePostTaskAsync(request);
        }
    }
}