﻿using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Microsoft.Owin;
using SimpleInjector.Integration.Owin;

namespace Keylol.Controllers.User
{
    /// <summary>
    ///     用户 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user")]
    public partial class UserController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;
        private readonly GeetestProvider _geetest;
        private readonly IOwinContext _owinContext;
        private readonly StatisticsProvider _statistics;
        private readonly KeylolUserManager _userManager;
        private readonly OneTimeTokenProvider _oneTimeToken;

        /// <summary>
        ///     创建 <see cref="UserController" />
        /// </summary>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="statistics">
        ///     <see cref="StatisticsProvider" />
        /// </param>
        /// <param name="geetest">
        ///     <see cref="GeetestProvider" />
        /// </param>
        /// <param name="owinContextProvider">
        ///     <see cref="OwinContextProvider" />
        /// </param>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        /// <param name="oneTimeToken"><see cref="OneTimeTokenProvider"/></param>
        public UserController(CouponProvider coupon, StatisticsProvider statistics, GeetestProvider geetest,
            OwinContextProvider owinContextProvider, KeylolDbContext dbContext, KeylolUserManager userManager,
            OneTimeTokenProvider oneTimeToken)
        {
            _coupon = coupon;
            _statistics = statistics;
            _geetest = geetest;
            _dbContext = dbContext;
            _userManager = userManager;
            _oneTimeToken = oneTimeToken;
            _owinContext = owinContextProvider.Current;
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
        //                var result = await _userManager.CreateAsync(user, model.Password);
        //                if (result.Succeeded)
        //                {
        //                    await SignInManager.SignInAsync(user, false, false);
        //
        //                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //                    // Send an email with this link
        //                    // string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                    // await _userManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
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
        //            var result = await _userManager.ConfirmEmailAsync(userId, code);
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
        //                var user = await _userManager.FindByNameAsync(model.Email);
        //                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
        //                {
        //                    // Don't reveal that the user does not exist or is not confirmed
        //                    return View("ForgotPasswordConfirmation");
        //                }
        //
        //                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //                // Send an email with this link
        //                // string code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
        //                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        //                // await _userManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
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
        //            var user = await _userManager.FindByNameAsync(model.Email);
        //            if (user == null)
        //            {
        //                // Don't reveal that the user does not exist
        //                return RedirectToAction("ResetPasswordConfirmation", "Account");
        //            }
        //            var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
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
        //            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(userId);
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
        //                var result = await _userManager.CreateAsync(user);
        //                if (result.Succeeded)
        //                {
        //                    result = await _userManager.AddLoginAsync(user.Id, info.Login);
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