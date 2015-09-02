using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Keylol
{
    public static class Extensions
    {
        public static int ByteLength(this string str)
        {
            var s = 0;
            for (var i = str.Length - 1; i >= 0; i--)
            {
                var code = (int) str[i];
                if (code <= 0xff) s++;
                else if (code > 0xff && code <= 0xffff) s += 2;
                if (code >= 0xDC00 && code <= 0xDFFF)
                {
                    i--;
                    s++;
                }
            }
            return s;
        }
    }

    public static class StatusClaim
    {
        public const string ClaimType = "status";

        public const string Probationer = "probationer";

        // Exclude this claim = "normal"

        public static async Task<string> GetStatusClaimAsync(this KeylolUserManager manager, string userId)
        {
            return (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType)?.Value;
        }

        public static async Task<IdentityResult> RemoveStatusClaimAsync(this KeylolUserManager manager, string userId)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                return await manager.RemoveClaimAsync(userId, claim);
            }
            return new IdentityResult("User doesn't have any status claims.");
        }

        public static async Task<IdentityResult> SetStatusClaimAsync(this KeylolUserManager manager, string userId,
            string status)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                await manager.RemoveClaimAsync(userId, claim);
            }
            return await manager.AddClaimAsync(userId, new Claim(ClaimType, status));
        }
    }

    public static class StaffClaim
    {
        public const string ClaimType = "staff";

        public const string Manager = "manager";
        public const string Moderator = "moderator";
        public const string Operator = "operator";

        // Exclude this claim = "user"

        public static async Task<string> GetStaffClaimAsync(this KeylolUserManager manager, string userId)
        {
            return (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType)?.Value;
        }

        public static async Task<IdentityResult> RemoveStaffClaimAsync(this KeylolUserManager manager, string userId)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                return await manager.RemoveClaimAsync(userId, claim);
            }
            return new IdentityResult("User doesn't have any staff claims.");
        }

        public static async Task<IdentityResult> SetStaffClaimAsync(this KeylolUserManager manager, string userId,
            string staff)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                await manager.RemoveClaimAsync(userId, claim);
            }
            return await manager.AddClaimAsync(userId, new Claim(ClaimType, staff));
        }
    }
}