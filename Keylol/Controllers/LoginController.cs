using System.Data.Entity;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity.Owin;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("login")]
    public class LoginController : KeylolApiController
    {
        /// <summary>
        /// 使用密码登录
        /// </summary>
        /// <param name="vm">登录所需相关属性</param>
        [AllowAnonymous]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(LoginLogDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(LoginVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var geetest = new Geetest();
            if (!await geetest.ValidateAsync(vm.GeetestChallenge, vm.GeetestSeccode, vm.GeetestValidate))
            {
                ModelState.AddModelError("authCode", "true");
                return BadRequest(ModelState);
            }
            var user = Regex.IsMatch(vm.EmailOrIdCode, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}$",
                RegexOptions.IgnoreCase)
                ? await UserManager.FindByEmailAsync(vm.EmailOrIdCode)
                : await DbContext.Users.SingleOrDefaultAsync(keylolUser => keylolUser.IdCode == vm.EmailOrIdCode);
            if (user == null)
            {
                ModelState.AddModelError("vm.EmailOrIdCode", "User doesn't exist.");
                return BadRequest(ModelState);
            }
            var result = await SignInManager.PasswordSignInAsync(user.UserName, vm.Password, true, true);
            switch (result)
            {
                case SignInStatus.Success:
                    var loginLog = new LoginLog
                    {
                        Ip = OwinContext.Request.RemoteIpAddress,
                        User = user
                    };
                    DbContext.LoginLogs.Add(loginLog);
                    await DbContext.SaveChangesAsync();
                    return Created($"login/{loginLog.Id}", new LoginLogDTO(loginLog));

                case SignInStatus.LockedOut:
                    ModelState.AddModelError("vm.EmailOrIdCode", "The user is locked out temporarily.");
                    break;

                case SignInStatus.Failure:
                    ModelState.AddModelError("vm.Password", "Password is not correct.");
                    break;

                default:
                    ModelState.AddModelError("vm.Email", "Login failed.");
                    break;
            }
            return BadRequest(ModelState);
        }
        
        /// <summary>
        /// 使用 SteamLoginToken 登录
        /// </summary>
        /// <param name="steamLoginTokenId">SteamLoginToken ID</param>
        [AllowAnonymous]
        [Route("token/{steamLoginTokenId}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(LoginLogDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "指定 SteamLoginToken 无效或未经过授权")]
        public async Task<IHttpActionResult> Post(string steamLoginTokenId)
        {
            var token = await DbContext.SteamLoginTokens.FindAsync(steamLoginTokenId);

            if (token == null)
                return Unauthorized();
            if (token.SteamId == null)
                return Unauthorized();

            var user = await DbContext.Users.SingleOrDefaultAsync(u => u.SteamId == token.SteamId);
            if (user == null)
                return Unauthorized();

            await SignInManager.SignInAsync(user, true, true);

            var loginLog = new LoginLog
            {
                Ip = OwinContext.Request.RemoteIpAddress,
                User = user
            };
            DbContext.LoginLogs.Add(loginLog);
            await DbContext.SaveChangesAsync();
            return Created($"login/{loginLog.Id}", new LoginLogDTO(loginLog));
        }
        
        /// <summary>
        /// 登出当前用户（清除 Cookies）
        /// </summary>
        [Route("current")]
        public IHttpActionResult Delete()
        {
            AuthenticationManager.SignOut();
            return Ok();
        }

        //        public IHttpActionResult Login(string returnUrl)
        //        {
        //            ViewBag.ReturnUrl = returnUrl;
        //            return View();
        //        }
        //        
        //        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //        {
        //            if (!ModelState.IsValid)
        //                return View(model);
        //
        //            // This doesn't count login failures towards account lockout
        //            // To enable password failures to trigger account lockout, change to shouldLockout: true
        //            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        //            switch (result)
        //            {
        //                case SignInStatus.Success:
        //                    return RedirectToLocal(returnUrl);
        //                case SignInStatus.LockedOut:
        //                    return View("Lockout");
        //                case SignInStatus.RequiresVerification:
        //                    return RedirectToAction("SendCode", new {ReturnUrl = returnUrl, model.RememberMe});
        //                case SignInStatus.Failure:
        //                default:
        //                    ModelState.AddModelError("", "Invalid login attempt.");
        //                    return View(model);
        //            }
        //        }
        //
        //        //
        //        // GET: /Account/VerifyCode
        //        [System.Web.Mvc.AllowAnonymous]
        //        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //        {
        //            // Require that the user has already logged in via username/password or external login
        //            if (!await SignInManager.HasBeenVerifiedAsync())
        //                return View("Error");
        //            return View(new VerifyCodeViewModel {Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe});
        //        }
        //
        //        //
        //        // POST: /Account/VerifyCode
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //                return View(model);
        //
        //            // The following code protects for brute force attacks against the two factor codes. 
        //            // If a user enters incorrect codes for a specified amount of time then the user account 
        //            // will be locked out for a specified amount of time. 
        //            // You can configure the account lockout settings in IdentityConfig
        //            var result =
        //                await
        //                    SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe,
        //                        model.RememberBrowser);
        //            switch (result)
        //            {
        //                case SignInStatus.Success:
        //                    return RedirectToLocal(model.ReturnUrl);
        //                case SignInStatus.LockedOut:
        //                    return View("Lockout");
        //                case SignInStatus.Failure:
        //                default:
        //                    ModelState.AddModelError("", "Invalid code.");
        //                    return View(model);
        //            }
        //        }
        //
        //        //
        //        // GET: /Account/Register
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult Register()
        //        {
        //            return View();
        //        }
        //
        //        //
        //        // POST: /Account/Register
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> Register(RegisterVM model)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                var user = new KeylolUser {UserName = model.Email, Email = model.Email};
        //                var result = await UserManager.CreateAsync(user, model.Password);
        //                if (result.Succeeded)
        //                {
        //                    await SignInManager.SignInAsync(user, false, false);
        //
        //                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //                    // Send an email with this link
        //                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                AddErrors(result);
        //            }
        //
        //            // If we got this far, something failed, redisplay form
        //            return View(model);
        //        }
        //
        //        //
        //        // GET: /Account/ConfirmEmail
        //        [System.Web.Mvc.AllowAnonymous]
        //        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        //        {
        //            if (userId == null || code == null)
        //                return View("Error");
        //            var result = await UserManager.ConfirmEmailAsync(userId, code);
        //            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        //        }
        //
        //        //
        //        // GET: /Account/ForgotPassword
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult ForgotPassword()
        //        {
        //            return View();
        //        }
        //
        //        //
        //        // POST: /Account/ForgotPassword
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                var user = await UserManager.FindByNameAsync(model.Email);
        //                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        //                {
        //                    // Don't reveal that the user does not exist or is not confirmed
        //                    return View("ForgotPasswordConfirmation");
        //                }
        //
        //                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //                // Send an email with this link
        //                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        //                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        //                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
        //            }
        //
        //            // If we got this far, something failed, redisplay form
        //            return View(model);
        //        }
        //
        //        //
        //        // GET: /Account/ForgotPasswordConfirmation
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult ForgotPasswordConfirmation()
        //        {
        //            return View();
        //        }
        //
        //        //
        //        // GET: /Account/ResetPassword
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult ResetPassword(string code)
        //        {
        //            return code == null ? View("Error") : View();
        //        }
        //
        //        //
        //        // POST: /Account/ResetPassword
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //                return View(model);
        //            var user = await UserManager.FindByNameAsync(model.Email);
        //            if (user == null)
        //            {
        //                // Don't reveal that the user does not exist
        //                return RedirectToAction("ResetPasswordConfirmation", "Account");
        //            }
        //            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
        //            if (result.Succeeded)
        //                return RedirectToAction("ResetPasswordConfirmation", "Account");
        //            AddErrors(result);
        //            return View();
        //        }
        //
        //        //
        //        // GET: /Account/ResetPasswordConfirmation
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult ResetPasswordConfirmation()
        //        {
        //            return View();
        //        }
        //
        //        //
        //        // POST: /Account/ExternalLogin
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult ExternalLogin(string provider, string returnUrl)
        //        {
        //            // Request a redirect to the external login provider
        //            return new ChallengeResult(provider,
        //                Url.Action("ExternalLoginCallback", "Account", new {ReturnUrl = returnUrl}));
        //        }
        //
        //        //
        //        // GET: /Account/SendCode
        //        [System.Web.Mvc.AllowAnonymous]
        //        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //        {
        //            var userId = await SignInManager.GetVerifiedUserIdAsync();
        //            if (userId == null)
        //                return View("Error");
        //            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //            var factorOptions =
        //                userFactors.Select(purpose => new SelectListItem {Text = purpose, Value = purpose}).ToList();
        //            return
        //                View(new SendCodeViewModel {Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe});
        //        }
        //
        //        //
        //        // POST: /Account/SendCode
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //                return View();
        //
        //            // Generate the token and send it
        //            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //                return View("Error");
        //            return RedirectToAction("VerifyCode",
        //                new {Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe});
        //        }
        //
        //        //
        //        // GET: /Account/ExternalLoginCallback
        //        [System.Web.Mvc.AllowAnonymous]
        //        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //        {
        //            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //            if (loginInfo == null)
        //                return RedirectToAction("Login");
        //
        //            // Sign in the user with this external login provider if the user already has a login
        //            var result = await SignInManager.ExternalSignInAsync(loginInfo, false);
        //            switch (result)
        //            {
        //                case SignInStatus.Success:
        //                    return RedirectToLocal(returnUrl);
        //                case SignInStatus.LockedOut:
        //                    return View("Lockout");
        //                case SignInStatus.RequiresVerification:
        //                    return RedirectToAction("SendCode", new {ReturnUrl = returnUrl, RememberMe = false});
        //                case SignInStatus.Failure:
        //                default:
        //                    // If the user does not have an account, then prompt the user to create an account
        //                    ViewBag.ReturnUrl = returnUrl;
        //                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //                    return View("ExternalLoginConfirmation",
        //                        new ExternalLoginConfirmationViewModel {Email = loginInfo.Email});
        //            }
        //        }
        //
        //        //
        //        // POST: /Account/ExternalLoginConfirmation
        //        [System.Web.Mvc.HttpPost]
        //        [System.Web.Mvc.AllowAnonymous]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
        //            string returnUrl)
        //        {
        //            if (User.Identity.IsAuthenticated)
        //                return RedirectToAction("Index", "Manage");
        //
        //            if (ModelState.IsValid)
        //            {
        //                // Get the information about the user from the external login provider
        //                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //                if (info == null)
        //                    return View("ExternalLoginFailure");
        //                var user = new KeylolUser {UserName = model.Email, Email = model.Email};
        //                var result = await UserManager.CreateAsync(user);
        //                if (result.Succeeded)
        //                {
        //                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //                    if (result.Succeeded)
        //                    {
        //                        await SignInManager.SignInAsync(user, false, false);
        //                        return RedirectToLocal(returnUrl);
        //                    }
        //                }
        //                AddErrors(result);
        //            }
        //
        //            ViewBag.ReturnUrl = returnUrl;
        //            return View(model);
        //        }
        //
        //        //
        //        // POST: /Account/LogOff
        //        [System.Web.Mvc.HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult LogOff()
        //        {
        //            AuthenticationManager.SignOut();
        //            return RedirectToAction("Index", "Home");
        //        }
        //
        //        //
        //        // GET: /Account/ExternalLoginFailure
        //        [System.Web.Mvc.AllowAnonymous]
        //        public ActionResult ExternalLoginFailure()
        //        {
        //            return View();
        //        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        //        private const string XsrfKey = "XsrfId";
        //
        //        private IAuthenticationManager AuthenticationManager
        //        {
        //            get { return HttpContext.GetOwinContext().Authentication; }
        //        }
        //
        //        private void AddErrors(IdentityResult result)
        //        {
        //            foreach (var error in result.Errors)
        //                ModelState.AddModelError("", error);
        //        }
        //
        //        private ActionResult RedirectToLocal(string returnUrl)
        //        {
        //            if (Url.IsLocalUrl(returnUrl))
        //                return Redirect(returnUrl);
        //            return RedirectToAction("Index", "Home");
        //        }
        //
        //        internal class ChallengeResult : HttpUnauthorizedResult
        //        {
        //            public ChallengeResult(string provider, string redirectUri)
        //                : this(provider, redirectUri, null) {}
        //
        //            public ChallengeResult(string provider, string redirectUri, string userId)
        //            {
        //                LoginProvider = provider;
        //                RedirectUri = redirectUri;
        //                UserId = userId;
        //            }
        //
        //            public string LoginProvider { get; set; }
        //            public string RedirectUri { get; set; }
        //            public string UserId { get; set; }
        //
        //            public override void ExecuteResult(ControllerContext context)
        //            {
        //                var properties = new AuthenticationProperties {RedirectUri = RedirectUri};
        //                if (UserId != null)
        //                    properties.Dictionary[XsrfKey] = UserId;
        //                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        //            }
        //        }

        #endregion
    }
}