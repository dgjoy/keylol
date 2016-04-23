using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Keylol.Identity
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class KeylolUserValidator : UserValidator<KeylolUser>
    {
        public KeylolUserValidator(UserManager<KeylolUser, string> manager) : base(manager)
        {
        }

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