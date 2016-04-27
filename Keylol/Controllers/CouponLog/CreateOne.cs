using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponLog
{
    public partial class CouponLogController
    {
        /// <summary>
        ///     人工增加一条文券变动日志，同时更新目标用户文券（文券日志类型为“其他”）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="change">文券变动量</param>
        /// <param name="description">变动描述</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> CreateOne(string userId, int change, string description)
        {
            try
            {
                await _coupon.Update(userId, CouponEvent.其他, change, description);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}