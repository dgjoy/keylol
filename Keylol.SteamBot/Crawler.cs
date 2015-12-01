using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SteamKit2;

namespace Keylol.SteamBot
{
    public class Crawler
    {
        public const string SteamCommunityDomain = "steamcommunity.com";
        public const string SteamCommunityUrlBase = "http://steamcommunity.com/";
        public const string ApiKey = "6C1D5F7CE0128FC918A64500C3B25AAB";
        public const string TuringRobotApiKey = "51c3bd1bb6a9d092f8b63aca01262edf";

        public CookieContainer Cookies { get; } = new CookieContainer();
        
        public HttpClient HttpClient { get; } = new HttpClient {Timeout = TimeSpan.FromSeconds(15)};

        public async Task<HttpWebResponse> RequestAsync(string url, string method, NameValueCollection data = null,
            bool ajax = true, string referer = "")
        {
            // Append the data to the URL for GET-requests
            var isGetMethod = method.ToLower() == "get";
            var dataString = data == null
                ? null
                : string.Join("&", Array.ConvertAll(data.AllKeys,
                    key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(data[key])}"));

            if (isGetMethod && !string.IsNullOrEmpty(dataString))
            {
                url += (url.Contains("?") ? "&" : "?") + dataString;
            }

            // Setup the request
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json, text/javascript;q=0.9, */*;q=0.5";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            // request.Host is set automatically
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Referer = string.IsNullOrEmpty(referer) ? "http://steamcommunity.com/trade/1" : referer;
            request.Timeout = 15000; // Timeout after 20 seconds
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            if (ajax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("X-Prototype-Version", "1.7");
            }

            // Cookies
            request.CookieContainer = Cookies;

            // Write the data to the body for POST and other methods
            if (!isGetMethod && !string.IsNullOrEmpty(dataString))
            {
                var dataBytes = Encoding.UTF8.GetBytes(dataString);
                request.ContentLength = dataBytes.Length;

                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                }
            }

            // Get the response
            return await request.GetResponseAsync() as HttpWebResponse;
        }
    }
}