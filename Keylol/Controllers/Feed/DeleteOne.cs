using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 删除一个 Feed
        /// </summary>
        /// <param name="id">Feed ID</param>
        [Route("{id}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Feed 不存在")]
        public async Task<IHttpActionResult> DeleteOneSlideshowEntry(int id)
        {
            var feed = await _dbContext.Feeds.FindAsync(id);
            if (feed == null)
                return NotFound();
            // TODO
//            _dbContext.Feeds.Remove(feed);
//            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}