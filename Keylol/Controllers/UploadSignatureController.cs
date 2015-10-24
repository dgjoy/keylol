using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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
        [ResponseType(typeof(string))]
        [SwaggerResponse(400, "请求 Policy 无效")]
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
            return Ok(BitConverter.ToString(hash).Replace("-", string.Empty).ToLower());
        }
    }
}