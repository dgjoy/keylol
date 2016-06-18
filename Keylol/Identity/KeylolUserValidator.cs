using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity
{
    /// <summary>
    ///     ASP.NET Identity UserValidator Keylol implementation
    /// </summary>
    public class KeylolUserValidator : IIdentityValidator<KeylolUser>
    {
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="KeylolUserValidator" />
        /// </summary>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public KeylolUserValidator(KeylolUserManager userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     Validates a user before saving
        /// </summary>
        /// <param name="user" />
        /// <returns />
        public async Task<IdentityResult> ValidateAsync(KeylolUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!Regex.IsMatch(user.IdCode, @"^[A-Z0-9]{5}$"))
                return IdentityResult.Failed(Errors.InvalidIdCode);

            if (user.UserName.Length < 3 || user.UserName.Length > 16)
                return IdentityResult.Failed(Errors.UserNameInvalidLength);

            if (!Regex.IsMatch(user.UserName, @"^[0-9A-Za-z\u4E00-\u9FCC]+$"))
                return IdentityResult.Failed(Errors.UserNameInvalidCharacter);

            if (string.IsNullOrWhiteSpace(user.Email))
                user.Email = null;
            if (!new EmailAddressAttribute().IsValid(user.Email))
                return IdentityResult.Failed(Errors.InvalidEmail);

            if (user.GamerTag.Length > 40)
                return IdentityResult.Failed(Errors.GamerTagInvalidLength);

            if (!Helpers.IsTrustedUrl(user.AvatarImage))
                return IdentityResult.Failed(Errors.AvatarImageUntrusted);

            if (!Helpers.IsTrustedUrl(user.HeaderImage))
                return IdentityResult.Failed(Errors.HeaderImageUntrusted);

            var idCodeOwner = await _userManager.FindByIdCodeAsync(user.IdCode);
            if (idCodeOwner == null && IsIdCodeReserved(user.IdCode))
            {
                return IdentityResult.Failed(Errors.IdCodeUsed);
            }
            if (idCodeOwner != null && idCodeOwner.Id != user.Id)
            {
                return IdentityResult.Failed(Errors.IdCodeUsed);
            }

            var userNameOwner = await _userManager.FindByNameAsync(user.UserName);
            if (userNameOwner != null && userNameOwner.Id != user.Id)
                return IdentityResult.Failed(Errors.UserNameUsed);

            if (user.Email != null)
            {
                var emailOwner = await _userManager.FindByEmailAsync(user.Email);
                if (emailOwner != null && emailOwner.Id != user.Id)
                    return IdentityResult.Failed(Errors.EmailUsed);
            }
            return IdentityResult.Success;
        }

        /// <summary>
        /// 判断指定识别码是否被保留
        /// </summary>
        /// <param name="idCode">识别码</param>
        /// <returns>如果识别码被保留，返回 <c>true</c></returns>
        public static bool IsIdCodeReserved(string idCode)
        {
            if (new[]
            {
                @"^([A-Z0-9])\1{4}$",
                @"^0000\d$",
                @"^\d0000$",
                @"^TEST.$",
                @"^.TEST$"
            }.Any(pattern => Regex.IsMatch(idCode, pattern)))
                return true;

            if (new[]
            {
                "12345",
                "54321",
                "ADMIN",
                "STAFF",
                "KEYLO",
                "KYLOL",
                "KEYLL",
                "VALVE",
                "STEAM",
                "CHINA",
                "JAPAN"
            }.Contains(idCode))
                return true;

            return false;
        }
    }
}