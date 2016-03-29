using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    /// 数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
    }
}