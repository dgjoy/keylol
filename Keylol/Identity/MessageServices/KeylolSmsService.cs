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

        private readonly RestClient _restClient = new RestClient("http://www.sendcloud.net/smsapi/send");
        private readonly string _smsUser = "keylol_sms_master";
        private readonly string _smsKey = "mjT9V5LPXS8A4iCBHtuSbBcqoY0Xxglt";
        private readonly string _templateID = "7367";
        private readonly string _msgType = "0";
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
            request.AddParameter("smsUser", _smsUser);
            request.AddParameter("templateId",_templateID);
            request.AddParameter("msgType", _msgType);
            await _restClient.ExecutePostTaskAsync(request);
        }
    }
}