using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     获取五个最近活跃的据点
        /// </summary>
        [Route("active")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<NormalPointDto>))]
        public async Task<IHttpActionResult> GetListByRecentActivity()
        {
            return Ok((await _dbContext.NormalPoints.AsNoTracking()
                .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                .ToListAsync()).Select(point => new NormalPointDto(point)));
        }
    }
}