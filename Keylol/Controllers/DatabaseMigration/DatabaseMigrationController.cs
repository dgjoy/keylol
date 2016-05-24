using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize(Users = "Stackia")]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        /// <summary>
        /// 添加一个据点职员
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="staffId">职员 ID</param>
        /// <returns></returns>
        [Route("add-point-staff")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPointStaff(string pointId, string staffId)
        {
            var dbContext = Global.Container.GetInstance<KeylolDbContext>();
            dbContext.PointStaff.Add(new PointStaff
            {
                PointId = pointId,
                StaffId = staffId
            });
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        ///     执行迁移
        /// </summary>
        [Route("go")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate()
        {
            var roleManager = Global.Container.GetInstance<KeylolRoleManager>();
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Operator));

            var dbContext = Global.Container.GetInstance<KeylolDbContext>();
            var operators = await dbContext.Database
                .SqlQuery<string>("SELECT UserId FROM dbo.UserClaims WHERE ClaimType = 'staff'")
                .ToListAsync();

            var userManager = Global.Container.GetInstance<KeylolUserManager>();
            foreach (var @operator in operators)
            {
                await userManager.AddToRoleAsync(@operator, KeylolRoles.Operator);
            }

            var usersToUnbindBot = await dbContext.Database
                .SqlQuery<string>("SELECT UserId FROM dbo.UserClaims WHERE ClaimType = 'status'")
                .ToListAsync();
            foreach (var userId in usersToUnbindBot)
            {
                var user = await userManager.FindByIdAsync(userId);
                user.SteamBotId = null;
            }
            await dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM dbo.UserClaims");
            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}