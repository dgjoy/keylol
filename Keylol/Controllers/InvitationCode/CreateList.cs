using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.InvitationCode
{
    public partial class InvitationCodeController
    {
        private readonly Dictionary<string, string> _sourceLookup = new Dictionary<string, string>
        {
            {"1001", "STCN-CREATOR"},
            {"1201", "ZHIHU-CREATOR"},
            {"1401", "TIEBA-CREATOR"},
            {"1501", "EMAIL-CREATOR"},
            {"2001", "STCN-LOTTERY"},
            {"2002", "STCN-LEADER"},
            {"2301", "TAOBAO-BUYER"},
            {"2302", "SONKWO-BUYER"},
            {"2501", "EVENT-INDIE1122"},
            {"2601", "SCUT-RELATIVES"},
            {"3001", "STCN-STAFF"},
            {"3101", "KYLO-STAFF"},
            {"3102", "KYLO-TEST"}
        };

        /// <summary>
        ///     生成邀请码
        /// </summary>
        /// <param name="prefix">邀请码前缀</param>
        /// <param name="number">生成数量，最大 20000，默认 1</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateList(string prefix, int number = 1)
        {
            if (number > 20000) number = 20000;
            if (!_sourceLookup.ContainsKey(prefix))
            {
                ModelState.AddModelError("sourceCode", "邀请码前缀无效");
                return BadRequest(ModelState);
            }
            var source = _sourceLookup[prefix];

            var random = new Random();
            var codes = new List<Models.InvitationCode>();
            for (var i = 0; i < number; i++)
            {
                codes.Add(new Models.InvitationCode
                {
                    Id = $"{prefix}-{random.Next(0, 10000).ToString("D4")}-{random.Next(0, 10000).ToString("D4")}",
                    Source = source
                });
            }
            DbContext.InvitationCodes.AddRange(codes);
            await DbContext.SaveChangesAsync();
            return Created("invitation-code", $"{source}\n{string.Join("\n", codes.Select(c => c.Id))}");
        }
    }
}