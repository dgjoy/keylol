using System;
using System.Threading.Tasks;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity
{
    /// <summary>
    /// ASP.NET Identity PasswordValidator Keylol implementation
    /// </summary>
    public class KeylolPasswordValidator : IIdentityValidator<string>
    {
        /// <summary>
        /// Validate the password
        /// </summary>
        /// <param name="password"/>
        /// <returns/>
        public Task<IdentityResult> ValidateAsync(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(password))
                return Task.FromResult(IdentityResult.Failed(Errors.PasswordAllWhitespace));

            if (password.Length < 6)
                return Task.FromResult(IdentityResult.Failed(Errors.PasswordTooShort));

            return Task.FromResult(IdentityResult.Success);
        }
    }
}