using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Keylol.ServiceBase;
using Keylol.Utilities;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供 SteamCN 相关操作服务
    /// </summary>
    public static class SteamCnProvider
    {
        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";

        private static readonly Encoding Charset = Encoding.GetEncoding("GBK");
        private const string UCenterEndpoint = "http://steamcn.com/uc_server/index.php";
        private const string UCenterKey = "OcB4u207q2dAF217j8x7u8SbB0u6T3zb52Xek6M7idR0Rf0782N7l3B757l8M4J2";
        private const string UCenterReleaseDate = "20110501";
        private const string UCenterAppId = "8";

        /// <summary>
        /// 用户登录结果
        /// </summary>
        public class UserLoginResult
        {
            /// <summary>
            /// UID
            /// </summary>
            public long Uid { get; set; }

            /// <summary>
            /// 用户名
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// 密码
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// 邮箱
            /// </summary>
            public string Email { get; set; }
        }

        /// <summary>
        /// 进行用户登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="isUid">用户名是否是 UID</param>
        /// <returns><see cref="UserLoginResult"/></returns>
        public static async Task<UserLoginResult> UserLoginAsync(string userName, string password, bool isUid)
        {
            var arguments = new Dictionary<string, string>
            {
                ["username"] = userName,
                ["password"] = password
            };
            if (isUid)
                arguments["isuid"] = "1";
            var result = await InvokeAsync("user", "login", arguments);
            var root = result?["root"];
            if (root == null) return null;
            return new UserLoginResult
            {
                Uid = long.Parse(root.ChildNodes[0].InnerText),
                UserName = root.ChildNodes[1].InnerText,
                Password = root.ChildNodes[2].InnerText,
                Email = root.ChildNodes[3].InnerText
            };
        }

        private static async Task<XmlDocument> InvokeAsync(string model, string action,
            Dictionary<string, string> arguments)
        {
            try
            {
                var request = WebRequest.CreateHttp(UCenterEndpoint);
                request.Method = WebRequestMethods.Http.Post;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = UserAgent;
                arguments["agent"] = Helpers.Md5(UserAgent, Charset);
                arguments["time"] = DateTime.Now.ToTimestamp().ToString();
                var postData = Charset.GetBytes(DictionaryToQueryString(new Dictionary<string, string>
                {
                    ["input"] =
                        PhpUrlEncode(AuthCode(DictionaryToQueryString(arguments), AuthCodeMethod.Encode, UCenterKey)),
                    ["m"] = model,
                    ["a"] = action,
                    ["release"] = UCenterReleaseDate,
                    ["appid"] = UCenterAppId
                }));
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(postData, 0, postData.Length);
                }
                using (var response = await request.GetResponseAsync())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null) return null;
                    var xml = new XmlDocument();
                    xml.Load(new StreamReader(responseStream, Charset));
                    return xml;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region 加解密相关

        private static string AuthCode(string sourceStr, AuthCodeMethod operation, string keyStr, int expiry = 0)
        {
            const int ckeyLength = 4;
            var source = Charset.GetBytes(sourceStr);
            var key = Charset.GetBytes(keyStr);

            key = Md5(key);

            var keya = Md5(SubBytes(key, 0, 0x10));
            var keyb = Md5(SubBytes(key, 0x10, 0x10));
            var keyc = (operation == AuthCodeMethod.Decode)
                ? SubBytes(source, 0, ckeyLength)
                : RandomBytes(ckeyLength);

            var cryptkey = AddBytes(keya, Md5(AddBytes(keya, keyc)));
            var keyLength = cryptkey.Length;

            if (operation == AuthCodeMethod.Decode)
            {
                while (source.Length%4 != 0)
                {
                    source = AddBytes(source, Charset.GetBytes("="));
                }
                source = Convert.FromBase64String(BytesToString(SubBytes(source, ckeyLength)));
            }
            else
            {
                source = AddBytes(expiry != 0
                    ? Charset.GetBytes((expiry + DateTime.Now.ToTimestamp()).ToString())
                    : Charset.GetBytes("0000000000"), SubBytes(Md5(AddBytes(source, keyb)), 0, 0x10), source);
            }

            var sourceLength = source.Length;

            var box = new int[256];
            for (var k = 0; k < 256; k++)
            {
                box[k] = k;
            }

            var rndkey = new int[256];
            for (var i = 0; i < 256; i++)
            {
                rndkey[i] = cryptkey[i%keyLength];
            }

            for (int j = 0, i = 0; i < 256; i++)
            {
                j = (j + box[i] + rndkey[i])%256;
                var tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }

            var result = new byte[sourceLength];
            for (int a = 0, j = 0, i = 0; i < sourceLength; i++)
            {
                a = (a + 1)%256;
                j = (j + box[a])%256;
                var tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;

                result[i] = (byte) (source[i] ^ (box[(box[a] + box[j])%256]));
            }

            if (operation == AuthCodeMethod.Decode)
            {
                var time = long.Parse(BytesToString(SubBytes(result, 0, 10)));
                if ((time == 0 || time - DateTime.Now.ToTimestamp() > 0) &&
                    BytesToString(SubBytes(result, 10, 16)) ==
                    BytesToString(SubBytes(Md5(AddBytes(SubBytes(result, 26), keyb)), 0, 16)))
                {
                    return BytesToString(SubBytes(result, 26));
                }
                return string.Empty;
            }
            return $"{BytesToString(keyc)}{Convert.ToBase64String(result).Replace("=", string.Empty)}";
        }

        private static string DictionaryToQueryString(Dictionary<string, string> args)
        {
            var sb = new StringBuilder();
            foreach (var item in args)
            {
                if (sb.Length != 0) sb.Append('&');
                sb.Append($"{item.Key}={item.Value}");
            }
            return sb.ToString();
        }

        private static string BytesToString(byte[] bytes)
        {
            return Charset.GetString(bytes);
        }

        /// <summary>
        /// Byte数组相加
        /// </summary>
        /// <param name="bytes">数组</param>
        /// <returns></returns>
        private static byte[] AddBytes(params byte[][] bytes)
        {
            var index = 0;
            var length = 0;
            foreach (var b in bytes)
            {
                length += b.Length;
            }
            var result = new byte[length];

            foreach (var bs in bytes)
            {
                foreach (var b in bs)
                {
                    result[index++] = b;
                }
            }
            return result;
        }

        private static byte[] RandomBytes(int lens)
        {
            var chArray = new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q',
                'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
                'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
            };
            var length = chArray.Length;
            var result = new byte[lens];
            var random = new Random();
            for (var i = 0; i < lens; i++)
            {
                result[i] = (byte) chArray[random.Next(length)];
            }
            return result;
        }

        private static byte[] SubBytes(byte[] b, int start, int length = int.MaxValue)
        {
            if (start >= b.Length) return new byte[0];
            if (start < 0) start = 0;
            if (length < 0) length = 0;
            if (length > b.Length || start + length > b.Length) length = b.Length - start;
            var result = new byte[length];
            var index = 0;
            for (var k = start; k < start + length; k++)
            {
                result[index++] = b[k];
            }
            return result;
        }

        private static string PhpUrlEncode(string str)
        {
            var result = string.Empty;
            const string keys = "_-.1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (var i = 0; i < str.Length; i++)
            {
                var str4 = str.Substring(i, 1);
                if (keys.Contains(str4))
                {
                    result = result + str4;
                }
                else
                {
                    result = Charset.GetBytes(str4).Aggregate(result, (current, n) => current + "%" + n.ToString("X"));
                }
            }
            return result;
        }

        private static byte[] Md5(byte[] b)
        {
            var cryptHandler = new MD5CryptoServiceProvider();
            var hash = cryptHandler.ComputeHash(b);
            var ret = "";
            foreach (var a in hash)
            {
                if (a < 16)
                {
                    ret += "0" + a.ToString("x");
                }
                else
                {
                    ret += a.ToString("x");
                }
            }
            return Charset.GetBytes(ret);
        }

        private enum AuthCodeMethod
        {
            Encode,
            Decode,
        }

        #endregion
    }
}