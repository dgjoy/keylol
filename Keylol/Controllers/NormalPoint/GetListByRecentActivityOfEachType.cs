using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     获取每种据点类型下最近活跃的据点
        /// </summary>
        [Route("active-of-each-type")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(Dictionary<NormalPointType, List<NormalPointDto>>))]
        public async Task<IHttpActionResult> GetListByRecentActivityOfEachType()
        {
            return Ok(new Dictionary<NormalPointType, List<NormalPointDto>>
            {
                [NormalPointType.Game] = (await _dbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Game)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 10)
                    .ToListAsync()).Select(point => new NormalPointDto(point, true)).ToList(),
                [NormalPointType.Genre] = (await _dbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Genre)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                    .ToListAsync()).Select(point => new NormalPointDto(point, true)).ToList(),
                [NormalPointType.Manufacturer] = (await _dbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Manufacturer)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                    .ToListAsync()).Select(point => new NormalPointDto(point, true)).ToList()
            });
        }
    }
}