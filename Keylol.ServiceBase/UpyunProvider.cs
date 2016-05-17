using System;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keylol.ServiceBase
{
    /// <summary>
    ///     提供又拍云相关的一些方法
    /// </summary>
    public static class UpyunProvider
    {
        private static readonly string Bucket = ConfigurationManager.AppSettings["upyunBucket"] ?? string.Empty;
        private static readonly string Operator = ConfigurationManager.AppSettings["upyunOperator"] ?? string.Empty;
        private static readonly string PasswordHash = ConfigurationManager.AppSettings["upyunPasswordHash"] ?? string.Empty;

        /// <summary>
        ///     允许的最大图片尺寸（字节）
        /// </summary>
        public static int MaxImageSize { get; } = 5*1024*1024; // 5 MB

        /// <summary>
        ///     Upyun Form API Key
        /// </summary>
        public static string FormKey { get; } = ConfigurationManager.AppSettings["upyunFormKey"];

        /// <summary>
        ///     从又拍云格式的 URL 中提取文件名
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
            var request = WebRequest.CreateHttp($"http://v0.api.upyun.com/{Bucket}/{path}");
            request.Method = method;
            request.ContentLength = contentLength;
            request.Date = DateTime.Now;
            var signature = Helpers.Md5(
                $"{method}&/{Bucket}/{path}&{request.Date.ToUniversalTime().ToString("R")}&{contentLength}&{PasswordHash}");
            request.Headers[HttpRequestHeader.Authorization] = $"UpYun {Operator}:{signature}";
            request.Timeout = 15000; // 15s
            request.ReadWriteTimeout = 120000; // 120s
            return request;
        }

        /// <summary>
        ///     上传文件到又拍云空间，新上传的文件将被命名为 {file-md5}.{extension}
        /// </summary>
        /// <param name="fileData">文件的二进制数据</param>
        /// <param name="extension">文件扩展名</param>
        /// <param name="contentType">文件 MIME 类型，留空自动根据后缀识别</param>
        /// <returns>上传成功返回文件名，失败返回 null</returns>
        public static async Task<string> UploadFile(byte[] fileData, string extension, string contentType = null)
        {
            try
            {
                var fileHash = Helpers.Md5(fileData);
                if (string.IsNullOrWhiteSpace(extension))
                    throw new ArgumentException("Need file extension", nameof(extension));
                var request = CreateRequest(WebRequestMethods.Http.Put, $"{fileHash}.{extension}", fileData.LongLength);
                request.ContentType = contentType;
                request.Headers["Content-MD5"] = fileHash;
                using (var requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(fileData, 0, fileData.Length);
                }
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK ? $"{fileHash}.{extension}" : null;
                }
            }
            catch (WebException)
            {
                return null;
            }
        }
    }
}