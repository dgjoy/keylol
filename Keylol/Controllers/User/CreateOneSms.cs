using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CsQuery.ExtensionMethods.Internal;
using Keylol.Models;
using Keylol.Provider;
using Keylol.Utilities;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="phoneNumber">注册手机号码</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("sms/token")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneBySms(string phoneNumber)
        {
            // Phone Number validated?
            if (!Regex.IsMatch(phoneNumber, Constants.ChinesePhoneNumberConstraint))
            {
                return this.BadRequest(nameof(phoneNumber), Errors.InvalidPhoneNumber);
            }
            
            // phone number confirmed?
            var user = await _userManager.FindByPhoneNumberAsync(phoneNumber);
            if (user != null && user.PhoneNumberConfirmed)
            {
                return this.BadRequest(nameof(phoneNumber),Errors.PhoneNumberUsed);
            }

            var smsCode = await _oneTimeToken.Generate(phoneNumber, TimeSpan.FromMinutes(3),
                OneTimeTokenPurpose.UserRegister,
                () => Task.FromResult(new Random().Next(999, 10000).ToString("D4")));

            // send sms
            return await SendSmsAsync(phoneNumber, smsCode);
        }

        private async Task<IHttpActionResult> SendSmsAsync(string phoneNumber, string code)
        {
            const string url = "http://www.sendcloud.net/smsapi/send";

            const string smsUser = "keylol_sms_master";
            const string smsKey = "mjT9V5LPXS8A4iCBHtuSbBcqoY0Xxglt";

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("smsUser", smsUser));
            paramList.Add(new KeyValuePair<string, string>("templateId", "7367"));
            paramList.Add(new KeyValuePair<string, string>("phone", phoneNumber));
            paramList.Add(new KeyValuePair<string, string>("msgType", "0"));
            paramList.Add(new KeyValuePair<string, string>("vars", "{\"%code%\":" + code + "}"));

            paramList.Sort(
                (p1, p2) => string.Compare(p1.Key, p2.Key, StringComparison.Ordinal)
            );

            var paramStr = paramList.Aggregate("", (current, param) => current + (param.Key.ToString() + "=" + param.Value.ToString() + "&"));

            var signStr = smsKey + "&" + paramStr + smsKey;
            var sign = GenerateMd5(signStr);

            paramList.Add(new KeyValuePair<string, string>("signature", sign));

            try
            {
                return Ok(await new HttpClient().PostAsync(url, new FormUrlEncodedContent(paramList)));
            }
            catch (Exception e)
            {
                return this.BadRequest(nameof(Exception), e.ToString());
            }
        }

        private static string GenerateMd5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(str));

            //get hash result after compute it
            var result = md5.Hash;

            var strBuilder = new StringBuilder();
            foreach (var s in result)
            {
                strBuilder.Append(s.ToString("x2"));
            }

            return strBuilder.ToString();
        }

    }
}