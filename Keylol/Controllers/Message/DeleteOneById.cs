using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models;
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
            var message = await _dbContext.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            if (!User.IsInRole(KeylolRoles.Operator) && (message.Type.IsMissiveMessage() || message.ReceiverId != userId))
                return Unauthorized();
            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}