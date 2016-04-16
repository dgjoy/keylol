using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Keylol.ServiceBase;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SteamKit2;

namespace Keylol.SteamBot
{
    public class BotCookieManager : IDisposable
    {
        private readonly RetryPolicy _retryPolicy;
        private readonly ILog _logger;
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private readonly Timer _checkTimer = new Timer(600000); // 10min
        private bool _disposed;

        public int BotSequenceNumber { get; set; }
        public EUniverse ConnectedUniverse { get; set; }
        public SteamID SteamId { get; set; }
        public ulong LoginKeyUniqueId { get; set; }
        public string WebApiUserNonce { get; set; }

        /// <summary>
        /// 检测到 Cookie 失效时触发
        /// </summary>
        public event EventHandler CookiesExpired;

        public BotCookieManager(RetryPolicy retryPolicy, ILogProvider logProvider)
        {
            _retryPolicy = retryPolicy;
            _logger = logProvider.Logger;

            _checkTimer.Elapsed += CheckTimerOnElapsed;
        }

        private async void CheckTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var valid = false;
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = CreateWebRequest("http://steamcommunity.com/");
                    request.Method = WebRequestMethods.Http.Get;
                    using (var response = (HttpWebResponse) await request.GetResponseAsync())
                    {
                        valid = response.Cookies["steamLogin"] == null ||
                                !response.Cookies["steamLogin"].Value.Equals("deleted",
                                    StringComparison.OrdinalIgnoreCase);
                    }
                });
            }
            catch (WebException e)
            {
                _logger.Warn($"#{BotSequenceNumber} Failed to validate cookies.", e);
            }
            if (valid) return;
            _logger.Info($"#{BotSequenceNumber} Cookies are expired.");
            OnCookiesExpired();
        }

        /// <summary>
        /// 重新获取新 Cookies，如果是第一次调用本方法，会同时启用定时检测计时器
        /// </summary>
        public async Task Refresh()
        {
            if (!_checkTimer.Enabled)
            {
                _logger.Info($"#{BotSequenceNumber} Cookies check timer started.");
                _checkTimer.Start();
            }

            if (string.IsNullOrEmpty(WebApiUserNonce))
                return;

            // generate an AES session key
            var sessionKey = CryptoHelper.GenerateRandomBlock(32);

            // RSA encrypt it with the public key for the universe we're on
            byte[] encryptedSessionKey;
            using (var rsa = new RSACrypto(KeyDictionary.GetPublicKey(ConnectedUniverse)))
            {
                encryptedSessionKey = rsa.Encrypt(sessionKey);
            }

            var loginKey = new byte[20];
            Array.Copy(Encoding.ASCII.GetBytes(WebApiUserNonce), loginKey, WebApiUserNonce.Length);

            // AES encrypt the loginkey with our session key
            var encryptedLoginKey = CryptoHelper.SymmetricEncrypt(loginKey, sessionKey);

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using (dynamic userAuth = WebAPI.GetAsyncInterface("ISteamUserAuth"))
                    {
                        KeyValue authResult =
                            await userAuth.AuthenticateUser(steamid: SteamId.ConvertToUInt64(),
                                sessionkey: HttpUtility.UrlEncode(encryptedSessionKey),
                                encrypted_loginkey: HttpUtility.UrlEncode(encryptedLoginKey),
                                method: "POST",
                                secure: true);

                        _cookieContainer.Add(new Cookie("sessionid",
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(LoginKeyUniqueId.ToString())),
                            string.Empty,
                            "steamcommunity.com"));

                        _cookieContainer.Add(new Cookie("steamLogin", authResult["token"].AsString(),
                            string.Empty,
                            "steamcommunity.com"));

                        _cookieContainer.Add(new Cookie("steamLoginSecure", authResult["tokensecure"].AsString(),
                            string.Empty,
                            "steamcommunity.com"));

                        _logger.Info($"#{BotSequenceNumber} Cookies refreshed.");
                    }
                });
            }
            catch (Exception e)
            {
                _logger.Warn($"#{BotSequenceNumber} Cookies refresh failed.", e);
            }
        }

        protected virtual void OnCookiesExpired()
        {
            CookiesExpired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 使用当前 Cookies 创建一个 <see cref="HttpWebRequest"/>，并伪装浏览器的 User Agent
        /// </summary>
        /// <param name="url">请求 URL</param>
        /// <returns><see cref="HttpWebRequest"/> 对象</returns>
        public HttpWebRequest CreateWebRequest(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Referer = url;
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml, application/json;q=0.9,*/*;q=0.8";
            request.Headers["Accept-Language"] = "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4";
            request.Timeout = 20000; // 20s
            request.ReadWriteTimeout = 100000; // 100s
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.CookieContainer = _cookieContainer;
            return request;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _checkTimer.Stop();
            }
            _disposed = true;
        }
    }
}