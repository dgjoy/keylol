using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 创建一个 Spotlight Point
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        [Route("spotlight-point")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneSpotlightPoint(string pointIdCode)
        {
            var point = await _dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return this.BadRequest(nameof(pointIdCode), Errors.NonExistent);
            _dbContext.Feeds.Add(new Models.Feed
            {
                StreamName = SpotlightPointStream.Name,
                EntryType = FeedEntryType.PointId,
                Entry = point.Id
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}