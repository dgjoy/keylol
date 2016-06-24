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
    public class GeetestProvider : IDisposable
    {
        private bool _disposed;
        private readonly string _key = ConfigurationManager.AppSettings["geetestKey"] ?? string.Empty;

        private readonly HttpClient _httpClient = new HttpClient
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
                var result = await _httpClient.PostAsync("validate.php", new FormUrlEncodedContent(postData));
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

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     资源清理
        /// </summary>
        /// <param name="disposing">是否清理托管对象</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposed = true;
        }
    }
}