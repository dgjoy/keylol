using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace Keylol.Identity
{
    /// <summary>
    /// ASP.NET Identity UserManager Keylol Implementation
    /// </summary>
    public class KeylolUserManager : UserManager<KeylolUser>
    {
        private KeylolUserManager(IUserStore<KeylolUser> store) : base(store)
        {
            UserValidator = new KeylolUserValidator(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            PasswordValidator = new PasswordValidator
            {
                RequireDigit = false,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false,
                RequiredLength = 6
            };

            UserLockoutEnabledByDefault = false;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            MaxFailedAccessAttemptsBeforeLockout = 10;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            //            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<KeylolUser>
            //            {
            //                MessageFormat = "Your security code is {0}"
            //            });
            //            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<KeylolUser>
            //            {
            //                Subject = "Security Code",
            //                BodyFormat = "Your security code is {0}"
            //            });
            EmailService = new EmailService();
            SmsService = new SmsService();
        }

        /// <summary>
        /// 创建 <see cref="KeylolUserManager"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public KeylolUserManager(KeylolDbContext dbContext) : this(new UserStore<KeylolUser>(dbContext))
        {
        }

        /// <summary>
        /// Create a user with the given password
        /// </summary>
        /// <param name="user"/><param name="password"/>
        /// <returns/>
        public override Task<IdentityResult> CreateAsync(KeylolUser user, string password)
        {
            user.ProfilePoint = new ProfilePoint();
            return base.CreateAsync(user, password);
        }
    }
}