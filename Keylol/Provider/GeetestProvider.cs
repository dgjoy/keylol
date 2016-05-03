using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Keylol.ServiceBase;

namespace Keylol.Provider
{
    /// <summary>
    ///     提供极验验证服务
    /// </summary>
    public class GeetestProvider
    {
        private readonly string _key = ConfigurationManager.AppSettings["geetestKey"];

        private HttpClient Client { get; } = new HttpClient
        {
            BaseAddress = new Uri("http://api.geetest.com/"),
            Timeout = TimeSpan.FromSeconds(2)
        };

        /// <summary>
        ///     验证 Challenge / Seccode / Validate 组合是否正确
        /// </summary>
        /// <param name="challenge">Challenge</param>
        /// <param name="seccode">Seccode</param>
        /// <param name="validate">Validate</param>
        /// <returns>是否验证通过</returns>
        public async Task<bool> ValidateAsync(string challenge, string seccode, string validate)
        {
            if (string.IsNullOrWhiteSpace(validate))
                return false;
            if (Helpers.Md5($"{_key}geetest{challenge}") != validate)
                return false;
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("seccode", seccode),
                new KeyValuePair<string, string>("sdk", "csharp_2.15.7.23.1")
            };
            try
            {
                var result = await Client.PostAsync("validate.php", new FormUrlEncodedContent(postData));
                result.EnsureSuccessStatusCode();
                if (await result.Content.ReadAsStringAsync() != Helpers.Md5(seccode))
                    return false;
            }
            catch (Exception)
            {
                // ignored
            }
            return true;
        }
    }
}