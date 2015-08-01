using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Keylol.DAL;
using Keylol.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace Keylol
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

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class KeylolUserManager : UserManager<KeylolUser>
    {
        public KeylolUserManager(IUserStore<KeylolUser> store)
            : base(store) {}

        public static KeylolUserManager Create(IdentityFactoryOptions<KeylolUserManager> options, IOwinContext context)
        {
            var manager = new KeylolUserManager(new UserStore<KeylolUser>(context.Get<KeylolDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<KeylolUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<KeylolUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<KeylolUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<KeylolUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class KeylolSignInManager : SignInManager<KeylolUser, string>
    {
        public KeylolSignInManager(KeylolUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager) {}

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(KeylolUser user)
        {
            return user.GenerateUserIdentityAsync((KeylolUserManager) UserManager, CookieAuthenticationDefaults.AuthenticationType);
        }

        public static KeylolSignInManager Create(IdentityFactoryOptions<KeylolSignInManager> options,
            IOwinContext context)
        {
            return new KeylolSignInManager(context.GetUserManager<KeylolUserManager>(), context.Authentication);
        }
    }
}