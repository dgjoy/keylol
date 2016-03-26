using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Message
{
    public partial class MessageController
    {
        /// <summary>
        /// 删除指定消息
        /// </summary>
        /// <param name="id">消息 ID</param>
        [Route("{id}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定消息不存在")]
        public async Task<IHttpActionResult> DeleteOneById(string id)
        {
            var message = await DbContext.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            DbContext.Messages.Remove(message);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}