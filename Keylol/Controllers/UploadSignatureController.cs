using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("upload-signature")]
    public class UploadSignatureController : KeylolApiController
    {
        private const string FormKey = "LaetquRR2LDCO0SezzqNNeTxjnQ=";
        
        /// <summary>
        /// 对上传请求进行签名
        /// </summary>
        /// <param name="policy">请求 Policy</param>
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "请求 Policy 无效")]
        public IHttpActionResult Post(string policy)
        {
            var options = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(policy)));

            if ((string) options["save-key"] != "{filemd5}{.suffix}")
                return BadRequest();

            if ((int)options["expiration"] > DateTime.Now.UnixTimestamp() + 120)
                return BadRequest();

            var range = ((string) options["content-length-range"]).Split(',').Select(int.Parse).ToList();
            if (range[1] > 5*1024*1024) // 5 MB
                return BadRequest();
            
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"{policy}&{FormKey}"));
            }
            return Created("upload-signature", BitConverter.ToString(hash).Replace("-", string.Empty).ToLower());
        }
    }
}