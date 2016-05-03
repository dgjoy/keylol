using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize(Roles = KeylolRoles.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        /// <summary>
        /// 执行迁移
        /// </summary>
        [Route("go")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate()
        {
            var roleManager = Startup.Container.GetInstance<KeylolRoleManager>();
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Operator));
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Moderator));
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Developer));
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Publisher));
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Manufacturer));
            await roleManager.CreateAsync(new IdentityRole(KeylolRoles.Staff));

            var dbContext = Startup.Container.GetInstance<KeylolDbContext>();
            var operators = await dbContext.Database
                .SqlQuery<string>("SELECT UserId FROM dbo.UserClaims WHERE ClaimType = 'staff'")
                .ToListAsync();

            var userManager = Startup.Container.GetInstance<KeylolUserManager>();
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