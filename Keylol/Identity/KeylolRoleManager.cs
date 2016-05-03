using Keylol.Models.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Identity
{
    /// <summary>
    ///     ASP.NET Identity RoleManager Keylol implementation
    /// </summary>
    public class KeylolRoleManager : RoleManager<IdentityRole>
    {
        /// <summary>
        ///     创建 <see cref="KeylolRoleManager" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        public KeylolRoleManager(KeylolDbContext dbContext) : base(new RoleStore<IdentityRole>(dbContext))
        {
        }
    }
}