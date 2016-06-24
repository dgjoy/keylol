using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Keylol.ServiceBase;
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
        [SwaggerResponse(HttpStatusCode.BadRequest, "请求 Policy 无效")]
        public IHttpActionResult CreateOne(string policy)
        {
            var options = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(policy)));

            if ((string) options["save-key"] != "{filemd5}{.suffix}")
                return BadRequest();

            if ((int) options["expiration"] > DateTime.Now.ToTimestamp() + 360)
                return BadRequest();

            var range = ((string) options["content-length-range"]).Split(',').Select(int.Parse).ToList();
            if (range[1] > UpyunProvider.MaxImageSize)
                return BadRequest();
            return Ok(Helpers.Md5($"{policy}&{UpyunProvider.FormKey}"));
        }
    }
}