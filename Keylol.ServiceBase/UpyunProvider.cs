using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keylol.ServiceBase
{
    /// <summary>
    /// 提供又拍云相关的一些方法
    /// </summary>
    public static class UpyunProvider
    {
        private const string ApiBase = "http://v0.api.upyun.com";
        private const string Bucket = "keylol";
        private const string Operator = "stackia";
        private const string PasswordHash = "9fed63e0ecf16aad31a9c3ccd31b0737";

        /// <summary>
        /// 从又拍云格式的 URL 中提取文件名
        /// </summary>
        /// <param name="url">要提取的 URL</param>
        /// <returns>提取到的文件名，如果提取失败返回 null</returns>
        public static string ExtractFileName(string url)
        {
            var match = Regex.Match(url,
                @"^(?:(?:(?:http:|https:)?\/\/(?:(?:keylol\.b0\.upaiyun)|(?:storage\.keylol))\.com\/)|(?:keylol:\/\/))?([a-z0-9\.]+?)(?:!.*)?$",
                RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static HttpWebRequest CreateRequest(string method, string path, long contentLength)
        {
            path = path.TrimStart('/');
            var request = WebRequest.CreateHttp($"{ApiBase}/{Bucket}/{path}");
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
            request.Timeout = 15000; // 15s
            request.ReadWriteTimeout = 120000; // 120s
            return request;
        }

        /// <summary>
        /// 上传文件到又拍云空间，新上传的文件将被命名为 {file-md5}.{extension}
        /// </summary>
        /// <param name="fileData">文件的二进制数据</param>
        /// <param name="extension">文件扩展名</param>
        /// <returns>上传成功返回文件名，失败返回 null</returns>
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