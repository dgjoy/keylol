using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Keylol.DAL;
using Keylol.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Keylol
{
    public class ClaimsAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private readonly string _claimType;
        private readonly string _claimValue;

        public ClaimsAuthorizeAttribute(string claimType, string claimValue)
        {
            _claimType = claimType;
            _claimValue = claimValue;
        }
        
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(0);
            }

            if (!principal.HasClaim(x => x.Type == _claimType && x.Value == _claimValue))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(0);
            }

            //User is Authorized, complete execution
            return Task.FromResult(0);
        }
    }

    public static class StatusClaim
    {
        public const string ClaimType = "status";

        public const string Probationer = "probationer";
        public const string Normal = "normal";

        public static async Task<string> GetStatusClaimAsync(this KeylolUserManager manager, string userId)
        {
            return (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType)?.Value;
        }

        public static async Task<IdentityResult> SetStatusClaimAsync(this KeylolUserManager manager, string userId,
            string status)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                await manager.RemoveClaimAsync(userId, claim);
            }
            return await manager.AddClaimAsync(userId, new Claim("status", status));
        }
    }

    public static class StaffClaim
    {
        public const string ClaimType = "staff";

        public const string Manager = "manager";
        public const string Moderator = "moderator";
        public const string Operator = "operator";

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
            var result = await base.ValidateAsync(user);
            if (errors.Any() || !result.Succeeded)
            {
                errors.AddRange(result.Errors);
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class KeylolUserManager : UserManager<KeylolUser>
    {
        public KeylolUserManager(IUserStore<KeylolUser> store)
            : base(store)
        {
        }

        public override Task<IdentityResult> CreateAsync(KeylolUser user, string password)
        {
            user.ProfilePoint = new ProfilePoint();
            return base.CreateAsync(user, password);
        }

        public static KeylolUserManager Create(IdentityFactoryOptions<KeylolUserManager> options, IOwinContext context)
        {
            var manager =
                new KeylolUserManager(new UserStore<KeylolUser>(context.Get<KeylolDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new KeylolUserValidator(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequireDigit = false,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false,
                RequiredLength = 6
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            manager.MaxFailedAccessAttemptsBeforeLockout = 10;

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
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(KeylolUser user)
        {
            return user.GenerateUserIdentityAsync((KeylolUserManager) UserManager);
        }

        public static KeylolSignInManager Create(IdentityFactoryOptions<KeylolSignInManager> options,
            IOwinContext context)
        {
            return new KeylolSignInManager(context.GetUserManager<KeylolUserManager>(), context.Authentication);
        }
    }
}