using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 更新一个 Spotlight Point
        /// </summary>
        /// <param name="id">Feed ID</param>
        /// <param name="pointIdCode">新的据点识别码</param>
        [Route("spotlight-point/{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Feed 不存在")]
        public async Task<IHttpActionResult> UpdateOneSpotlightPoint(int id, string pointIdCode)
        {
            var feed = await _dbContext.Feeds.FindAsync(id);
            if (feed == null || feed.StreamName != SpotlightPointStream.Name)
                return NotFound();
            var point = await _dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return this.BadRequest(nameof(pointIdCode), Errors.NonExistent);
            feed.Entry = point.Id;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}