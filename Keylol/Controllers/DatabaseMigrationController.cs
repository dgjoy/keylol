using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Keylol.Utilities;

namespace Keylol.Controllers
{
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
    }
}