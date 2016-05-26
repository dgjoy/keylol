using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 置顶一个 Feed（置顶相当于删除重推送，Feed ID 将会发生变化）
        /// </summary>
        /// <param name="id">Feed ID</param>
        [Route("slideshow-entry/top/{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Feed 不存在")]
        public async Task<IHttpActionResult> UpdateOneToTop(int id)
        {
            var feed = await _dbContext.Feeds.FindAsync(id);
            if (feed == null)
                return NotFound();
            _dbContext.Feeds.Remove(feed);
            await _dbContext.SaveChangesAsync();
            _dbContext.Feeds.Add(feed);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}