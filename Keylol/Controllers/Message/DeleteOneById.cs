using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Message
{
    public partial class MessageController
    {
        /// <summary>
        ///     删除指定消息
        /// </summary>
        /// <param name="id">消息 ID</param>
        [Route("{id}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定消息不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权删除这则消息")]
        public async Task<IHttpActionResult> DeleteOneById(string id)
        {
            var userId = User.Identity.GetUserId();
            var staffClaim = await UserManager.GetStaffClaimAsync(userId);
            var message = await DbContext.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            if (staffClaim != StaffClaim.Operator && (message.Type.IsMissiveMessage() || message.ReceiverId != userId))
                return Unauthorized();
            DbContext.Messages.Remove(message);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}