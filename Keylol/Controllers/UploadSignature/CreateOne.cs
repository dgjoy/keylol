using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Keylol.Utilities;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.UploadSignature
{
    public partial class UploadSignatureController
    {
        /// <summary>
        ///     对上传请求进行签名
        /// </summary>
        /// <param name="policy">请求 Policy</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "请求 Policy 无效")]
        public IHttpActionResult CreateOne(string policy)
        {
            var options = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(policy)));

            if ((string) options["save-key"] != "{filemd5}{.suffix}")
                return BadRequest();

            if ((int) options["expiration"] > DateTime.Now.UnixTimestamp() + 330)
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