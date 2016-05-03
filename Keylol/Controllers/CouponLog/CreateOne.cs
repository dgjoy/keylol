using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models;
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
        [Authorize(Roles = KeylolRoles.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> CreateOne(string userId, int change, string description)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            await _coupon.Update(user, CouponEvent.其他, change, description);
            return Ok();
        }
    }
}