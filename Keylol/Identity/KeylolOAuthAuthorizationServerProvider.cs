using System;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;

namespace Keylol.Identity
{
    /// <summary>
    /// OAuth Authorzation Server 实现
    /// </summary>
    public class KeylolOAuthAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "password". This occurs when the user has provided name and password
        ///             credentials directly into the client application's user interface, and the client application is using those to acquire an "access_token" and 
        ///             optional "refresh_token". If the web application supports the
        ///             resource owner credentials grant type it must validate the context.Username and context.Password as appropriate. To issue an
        ///             access token the context.Validated must be called with a new ticket containing the claims about the resource owner which should be associated
        ///             with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        ///             The default behavior is to reject this grant type.
        ///             See also http://tools.ietf.org/html/rfc6749#section-4.3.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var parameters = await context.Request.ReadFormAsync();

            var geetest = Startup.Container.GetInstance<GeetestProvider>();
            if (!await geetest.ValidateAsync(parameters["captcha_challenge"], parameters["captcha_seccode"],
                        parameters["captcha_validate"]))
            {
                context.SetError("invalid_captcha");
                return;
            }

            var userManager = context.OwinContext.Get<KeylolUserManager>();
            var dbContext = context.OwinContext.Get<KeylolDbContext>();
            var emailOrIdCode = context.UserName;
            var user = Regex.IsMatch(emailOrIdCode, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}$",
                RegexOptions.IgnoreCase)
                ? await userManager.FindByEmailAsync(emailOrIdCode)
                : await
                    dbContext.Users.SingleOrDefaultAsync(keylolUser => keylolUser.IdCode == emailOrIdCode);
            if (user == null)
            {
                context.SetError("username", "不存在此识别码或邮箱对应的用户");
                return;
            }
            if (userManager.CheckPassword(user, context.Password))
            {
                //                var loginLog = new LoginLog
                //                {
                //                    Ip = context.Request.RemoteIpAddress,
                //                    UserId = user.Id
                //                };
                //                _dbContext.LoginLogs.Add(loginLog);
                //                await _dbContext.SaveChangesAsync();
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                context.Validated(identity);
            }
//            var result = await SignInManager.PasswordSignInAsync(user.UserName, requestDto.Password, true, true);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    var loginLog = new LoginLog
//                    {
//                        Ip = _owinContext.Request.RemoteIpAddress,
//                        User = user
//                    };
//                    _dbContext.LoginLogs.Add(loginLog);
//                    await _dbContext.SaveChangesAsync();
//                    return Created($"login/{loginLog.Id}", new LoginLogDto(loginLog));
//
//                case SignInStatus.LockedOut:
//                    ModelState.AddModelError("vm.EmailOrIdCode", "The user is locked out temporarily.");
//                    break;
//
//                case SignInStatus.Failure:
//                    ModelState.AddModelError("vm.Password", "Password is not correct.");
//                    break;
//
//                default:
//                    ModelState.AddModelError("vm.Email", "Login failed.");
//                    break;
//            }
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of any other value. If the application supports custom grant types
        ///             it is entirely responsible for determining if the request should result in an access_token. If context.Validated is called with ticket
        ///             information the response body is produced in the same way as the other standard grant types. If additional response parameters must be
        ///             included they may be added in the final TokenEndpoint call.
        ///             See also http://tools.ietf.org/html/rfc6749#section-4.5
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            if (context.GrantType.Equals("steam_login_token", StringComparison.OrdinalIgnoreCase))
            {
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
        ///             present on the request. If the web application accepts Basic authentication credentials, 
        ///             context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
        ///             application accepts "client_id" and "client_secret" as form encoded POST parameters, 
        ///             context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
        ///             If context.Validated is not called the request will not proceed further. 
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                // 暂时不限制 Client ID
                context.Validated(clientId);
            }
            return Task.FromResult(0);
        }
    }
}