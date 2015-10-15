using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;

namespace Keylol.Controllers
{
    [Authorize]
    [Route("normal-point")]
    public class NormalPointController : KeylolApiController
    {
        public async Task<IHttpActionResult> Get(string keyword, int skip = 0, int take = 5)
        {
            return Ok((await DbContext.NormalPoints.SqlQuery(@"SELECT * FROM [dbo].[NormalPoints] AS [t1] INNER JOIN
	                (SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})		                UNION ALL		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})	                ) AS [t2] GROUP BY [t2].[KEY]) as [t3]                ON [t1].[Id] = [t3].[KEY]                ORDER BY [t3].[RANK] DESC
                OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync()).Select(point => new NormalPointDTO(point)));
        }
    }
}