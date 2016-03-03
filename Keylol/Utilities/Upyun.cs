using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Keylol.Utilities
{
    public static class Upyun
    {
        private const string APIBase = "http://v0.api.upyun.com";
        private const string Bucket = "keylol";
        private const string Operator = "stackia";
        private const string PasswordHash = "9fed63e0ecf16aad31a9c3ccd31b0737";

        public static string ExtractFileName(string uri)
        {
            var match = Regex.Match(uri,
                @"^(?:(?:(?:http:|https:)?\/\/(?:(?:keylol\.b0\.upaiyun)|(?:storage\.keylol))\.com\/)|(?:keylol:\/\/))?([a-z0-9\.]+?)(?:!.*)?$",
                RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string CustomVersionUrl(string fileName, string version = null)
        {
            var url = "//storage.keylol.com/" + fileName;
            if (!string.IsNullOrEmpty(version))
                url += "!" + version;
            return url;
        }

        private static HttpWebRequest CreateRequest(string method, string path, long contentLength)
        {
            path = path.TrimStart('/');
            var request = WebRequest.CreateHttp($"{APIBase}/{Bucket}/{path}");
            request.Method = method;
            request.ContentLength = contentLength;
            request.Date = DateTime.Now;
            var signature =
                $"{method}&/{Bucket}/{path}&{request.Date.ToUniversalTime().ToString("R")}&{contentLength}&{PasswordHash}";
            using (var md5 = MD5.Create())
            {
                signature =
                    BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(signature))).Replace("-", "").ToLower();
            }
            request.Headers[HttpRequestHeader.Authorization] = $"UpYun {Operator}:{signature}";
            return request;
        }

        public static async Task<string> UploadFile(byte[] fileData, string extension)
        {
            try
            {
                string fileHash;
                using (var md5 = MD5.Create())
                {
                    fileHash = BitConverter.ToString(md5.ComputeHash(fileData)).Replace("-", "").ToLower();
                }
                var request = CreateRequest(WebRequestMethods.Http.Put, $"{fileHash}{extension}", fileData.LongLength);
                using (var requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(fileData, 0, fileData.Length);
                }
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK ? $"{fileHash}{extension}" : null;
                }
            }
            catch (WebException)
            {
                return null;
            }
        }
    }
}