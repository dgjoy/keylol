using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("normal-point")]
    public class NormalPointController : KeylolApiController
    {
        [Route("{id}")]
        [ResponseType(typeof(NormalPointDTO))]
        public async Task<IHttpActionResult> Get(string id)
        {
            var point = await DbContext.NormalPoints.FindAsync(id);
            if (point == null)
                return NotFound();

            return Ok(new NormalPointDTO(point));
        }

        [Route]
        [ResponseType(typeof(List<NormalPointDTO>))]
        public async Task<IHttpActionResult> Get(string keyword, int skip = 0, int take = 5)
        {
            return Ok((await DbContext.NormalPoints.SqlQuery(@"SELECT * FROM [dbo].[NormalPoints] AS [t1] INNER JOIN (
                    SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
		                UNION ALL
		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})
	                ) AS [t2] GROUP BY [t2].[KEY]
                ) AS [t3] ON [t1].[Id] = [t3].[KEY]
                ORDER BY [t3].[RANK] DESC
                OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync()).Select(point => new NormalPointDTO(point)));
        }

        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [ResponseType(typeof(NormalPointDTO))]
        public async Task<IHttpActionResult> Post(NormalPointVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "Only 5 uppercase letters and digits are allowed in IdCode.");
                return BadRequest(ModelState);
            }
            if (await DbContext.NormalPoints.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "IdCode is already used by others.");
                return BadRequest(ModelState);
            }

            var normalPoint = DbContext.NormalPoints.Create();
            normalPoint.IdCode = vm.IdCode;
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.EnglishName = vm.EnglishName;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Type = vm.Type;
            if (normalPoint.Type == NormalPointType.Game)
            {
                if (string.IsNullOrEmpty(vm.StoreLink))
                {
                    ModelState.AddModelError("vm.StoreLink", "Invalid store link for game point.");
                    return BadRequest(ModelState);
                }
                normalPoint.StoreLink = vm.StoreLink;
            }

            DbContext.NormalPoints.Add(normalPoint);
            await DbContext.SaveChangesAsync();

            return Created($"normal-point/{normalPoint.Id}", new NormalPointDTO(normalPoint));
        }
    }
}