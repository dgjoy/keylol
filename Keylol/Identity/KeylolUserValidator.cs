using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity
{
    /// <summary>
    ///     ASP.NET Identity UserValidator Keylol implementation
    /// </summary>
    public class KeylolUserValidator : UserValidator<KeylolUser>
    {
        /// <summary>
        ///     创建 <see cref="KeylolUserValidator" />
        /// </summary>
        /// <param name="manager"><see cref="KeylolUserManager" /> 对象</param>
        public KeylolUserValidator(KeylolUserManager manager) : base(manager)
        {
        }

        /// <summary>
        ///     Validates a user before saving
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public override async Task<IdentityResult> ValidateAsync(KeylolUser user)
        {
            var errors = new List<string>();
            var byteLength = user.UserName.ByteLength();
            if (byteLength < 3 || byteLength > 16)
            {
                errors.Add("UserName should be 3-16 bytes.");
            }
            if (!AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, @"^[0-9A-Za-z\u4E00-\u9FCC]+$"))
            {
                errors.Add("Only digits, letters and Chinese characters are allowed in UserName.");
            }
            if (user.Email != null &&
                !Regex.IsMatch(user.Email, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}$", RegexOptions.IgnoreCase))
            {
                errors.Add("Email is invalid.");
            }
            if (user.GamerTag.ByteLength() > 40)
            {
                errors.Add("GamerTag should not be longer than 40 bytes.");
            }
            var result = await base.ValidateAsync(user);
            if (errors.Any() || !result.Succeeded)
            {
                errors.AddRange(result.Errors);
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }
    }
}