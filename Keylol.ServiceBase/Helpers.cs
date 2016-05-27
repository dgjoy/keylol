using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Keylol.ServiceBase
{
    /// <summary>
    ///     一些常用的帮助方法
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        ///     计算一段字符串的 MD5 值，使用 UTF-8 编码
        /// </summary>
        /// <param name="text">要计算的字符串</param>
        /// <returns>字符串的 MD5，用小写字母表示</returns>
        public static string Md5(string text)
        {
            return Md5(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        ///     计算字节数据的 MD5 值
        /// </summary>
        /// <param name="data">要计算的数据</param>
        /// <returns>数据的 MD5，用小写字母表示</returns>
        public static string Md5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(data)).Replace("-", string.Empty).ToLower();
            }
        }

        /// <summary>
        ///     从 Unix 时间戳创建 DateTime 对象
        /// </summary>
        /// <param name="unixTimeStamp">Unix 时间戳（秒）</param>
        /// <returns>创建的 DateTime 对象</returns>
        public static DateTime DateTimeFromTimeStamp(double unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
        }

        /// <summary>
        ///     检测 URL 是否是可信来源（以 keylol:// 为前缀）
        /// </summary>
        /// <param name="url">要检测的 URL</param>
        /// <param name="allowNullOrEmpty">是否允许 URL 为空</param>
        /// <returns>可信（allowNullOrEmpty 时 URL 为空也认为可信）返回 true，不可信返回 false</returns>
        public static bool IsTrustedUrl(string url, bool allowNullOrEmpty = true)
        {
            return (allowNullOrEmpty && string.IsNullOrEmpty(url)) || url.StartsWith("keylol://");
        }

        /// <summary>
        /// 安全反序列化（如果无法反序列化，返回 null）
        /// </summary>
        /// <param name="jsonText">JSON 文本</param>
        /// <typeparam name="T">结果类型</typeparam>
        /// <return>反序列化后的对象</return>
        public static T SafeDeserialize<T>(string jsonText)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonText);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}