using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        [Route]
        [HttpGet]
        public async Task<IHttpActionResult> Test()
        {
            var record = DbContext.UserGameRecords.Create();
            record.UserId = User.Identity.GetUserId();
            record.TotalPlayedTime = 2;
            record.SteamAppId = 999;
            DbContext.UserGameRecords.Add(record);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}