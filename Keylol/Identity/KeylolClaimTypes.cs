using System.Security.Claims;

namespace Keylol.Identity
{
    /// <summary>
    ///     提供常用的 Claim 类型名称
    /// </summary>
    public static class KeylolClaimTypes
    {
        /// <summary>
        ///     User ID
        /// </summary>
        public static string UserId { get; set; } = ClaimTypes.NameIdentifier;

        /// <summary>
        ///     User name
        /// </summary>
        public static string UserName { get; set; } = ClaimTypes.Name;

        /// <summary>
        ///     Role
        /// </summary>
        public static string Role { get; set; } = ClaimTypes.Role;

        /// <summary>
        ///     Security stamp
        /// </summary>
        public static string SecurityStamp { get; set; } = "Keylol.Identity.SecurityStamp";
    }
}