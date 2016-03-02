using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;

namespace Keylol.Controllers
{
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        // 迁移方法需要保证幂等性

        /// <summary>
        ///     v1.1.4: 把 NameInStore 属性提取至 SteamStoreNames 表中
        /// </summary>
        [Route("v1-1-4-steam-store-names")]
        [HttpPost]
        public async Task<IHttpActionResult> V114SteamStoreNames()
        {
            var points = await DbContext.NormalPoints
                .Include(p => p.SteamStoreNames)
                .Where(p => (p.Type == NormalPointType.Genre || p.Type == NormalPointType.Manufacturer) &&
                            !string.IsNullOrEmpty(p.NameInSteamStore))
                .ToListAsync();
            foreach (var normalPoint in points)
            {
                if (normalPoint.SteamStoreNames.Select(n => n.Name).Contains(normalPoint.NameInSteamStore))
                    continue;
                var name = await DbContext.SteamStoreNames
                    .Where(n => n.Name == normalPoint.NameInSteamStore)
                    .SingleOrDefaultAsync();
                if (name == null)
                {
                    name = DbContext.SteamStoreNames.Create();
                    name.Name = normalPoint.NameInSteamStore;
                }
                normalPoint.SteamStoreNames.Add(name);
            }
            await DbContext.SaveChangesAsync();
            return Ok("迁移成功");
        }

        /// <summary>
        ///     v1.1.4: 把封面图转换为使用 capsule 版本
        /// </summary>
        [Route("v1-1-4-cover-image-capsule")]
        [HttpPost]
        public async Task<IHttpActionResult> V114CoverImageCapsule()
        {
            var points = await DbContext.NormalPoints
                .Where(p => p.Type == NormalPointType.Game && !string.IsNullOrEmpty(p.CoverImage))
                .ToListAsync();
            foreach (var normalPoint in points)
            {
                var match = Regex.Match(normalPoint.CoverImage, @"^keylol://steam/app-headers/(\d+)$");
                if (match.Success)
                {
                    normalPoint.CoverImage = $"keylol://steam/app-capsules/{match.Groups[1].Value}";
                }
            }
            await DbContext.SaveChangesAsync();
            return Ok("转换成功");
        }
    }
}