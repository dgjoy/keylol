using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("user")]
    public class UserController : KeylolApiController
    {
        public enum IdType
        {
            Id,
            IdCode,
            UserName
        }

#if DEBUG
        /// <summary>
        /// 调试测试用
        /// </summary>
        [Route("test")]
        [HttpGet]
        public async Task<IHttpActionResult> Test()
        {
            await UserManager.SetStaffClaimAsync(User.Identity.GetUserId(), StaffClaim.Operator);
            return Ok();
        }
#endif


        /// <summary>
        /// 根据 Id、UserName 或者 IdCode 取得一名用户
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="includeProfilePointBackgroundImage">是否包含用户据点背景图片，默认 false</param>
        /// <param name="includeClaims">是否包含用户权限级别，默认 false</param>
        /// <param name="includeSecurity">是否包含用户安全信息（邮箱、登录保护等），用户只能获取自己的安全信息（除非是运维职员），默认 false</param>
        /// <param name="includeSteam">是否包含用户 Steam 信息，用户只能获取自己的 Steam 信息（除非是运维职员），默认 false</param>
        /// <param name="includeSteamBot">是否包含用户所属 Steam 机器人（用户只能获取自己的机器人（除非是运维职员），默认 false</param>
        /// <param name="includeSubscribeCount">是否包含用户订阅数量（用户只能获取自己的订阅信息（除非是运维职员），默认 false</param>
        /// <param name="includeStats">是否包含用户读者数和文章数，默认 false</param>
        /// <param name="includeSubscribed">是否包含该用户有没有被当前用户的信息，默认 false</param>
        /// <param name="includeMoreOptions">是否包含更多杂项设置（例如通知偏好设置），默认 false</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [ResponseType(typeof (UserDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> Get(string id, bool includeProfilePointBackgroundImage = false,
            bool includeClaims = false, bool includeSecurity = false, bool includeSteam = false,
            bool includeSteamBot = false, bool includeSubscribeCount = false, bool includeStats = false,
            bool includeSubscribed = false, bool includeMoreOptions = false, IdType idType = IdType.Id)
        {
            KeylolUser user;
            switch (idType)
            {
                case IdType.UserName:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.UserName == id);
                    break;

                case IdType.IdCode:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == id);
                    break;

                case IdType.Id:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.Id == id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            var visitorId = User.Identity.GetUserId();
            var visitorStaffClaim = await UserManager.GetStaffClaimAsync(visitorId);

            if (user == null)
                return NotFound();

            var userDTO = includeMoreOptions ? new UserWithMoreOptionsDTO(user) : new UserDTO(user);

            if (includeProfilePointBackgroundImage)
                userDTO.ProfilePointBackgroundImage = user.ProfilePoint.BackgroundImage;

            if (includeClaims)
            {
                userDTO.StatusClaim = await UserManager.GetStatusClaimAsync(user.Id);
                userDTO.StaffClaim = await UserManager.GetStaffClaimAsync(user.Id);
            }

            if (includeSecurity)
            {
                if (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator)
                    userDTO.IncludeSecurity();
            }

            if (includeSteam)
            {
                if (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator)
                    userDTO.IncludeSteam();
            }

            if (includeSteamBot)
            {
                if (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator)
                    userDTO.SteamBot = new SteamBotDTO(user.SteamBot);
            }

            if (includeSubscribeCount)
            {
                userDTO.SubscribedPointCount =
                    await DbContext.Users.Where(u => u.Id == user.Id).SelectMany(u => u.SubscribedPoints).CountAsync();
            }

            if (includeStats)
            {
                var stats = await DbContext.Users.Where(u => u.Id == user.Id)
                    .Select(u =>
                        new
                        {
                            subscriberCount = u.ProfilePoint.Subscribers.Count,
                            articleCount = u.ProfilePoint.Entries.OfType<Article>().Count()
                        })
                    .SingleOrDefaultAsync();
                userDTO.SubscriberCount = stats.subscriberCount;
                userDTO.ArticleCount = stats.articleCount;
            }

            if (includeSubscribed)
            {
                userDTO.Subscribed = await DbContext.Users.Where(u => u.Id == visitorId)
                    .SelectMany(u => u.SubscribedPoints)
                    .Select(p => p.Id)
                    .ContainsAsync(user.Id);
            }

            return Ok(userDTO);
        }

        /// <summary>
        /// 注册一个新用户
        /// </summary>
        /// <param name="vm">用户相关属性</param>
        [AllowAnonymous]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (LoginLogDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "邀请码无效")]
        public async Task<IHttpActionResult> Post(UserPostVM vm)
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
            var invitationCode = await DbContext.InvitationCodes.FindAsync(vm.InvitationCode);
            if (invitationCode == null || invitationCode.UsedByUser != null)
            {
                return Unauthorized();
            }
            var steamBindingToken = await DbContext.SteamBindingTokens.FindAsync(vm.SteamBindingTokenId);
            if (steamBindingToken == null)
            {
                ModelState.AddModelError("vm.SteamBindingTokenId", "Invalid steam binding token.");
                return BadRequest(ModelState);
            }
            if (await DbContext.Users.SingleOrDefaultAsync(u => u.SteamId == steamBindingToken.SteamId) != null)
            {
                ModelState.AddModelError("vm.SteamBindingTokenId",
                    "Steam account has been binded to another Keylol account.");
                return BadRequest(ModelState);
            }
            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "Only 5 uppercase letters and digits are allowed in IdCode.");
                return BadRequest(ModelState);
            }
            if (await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "IdCode is already used by others.");
                return BadRequest(ModelState);
            }
            if (await UserManager.FindByNameAsync(vm.UserName) != null)
            {
                ModelState.AddModelError("vm.UserName", "UserName is already used by others.");
                return BadRequest(ModelState);
            }
            var user = new KeylolUser
            {
                IdCode = vm.IdCode,
                UserName = vm.UserName,
                RegisterIp = OwinContext.Request.RemoteIpAddress,
                AvatarImage = vm.AvatarImage,
                SteamBindingTime = DateTime.Now,
                SteamId = steamBindingToken.SteamId,
                SteamProfileName = vm.SteamProfileName,
                SteamBotId = steamBindingToken.BotId,
                InvitationCode = invitationCode
            };

            var result = await UserManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Contains("UserName"))
                        ModelState.AddModelError("vm.UserName", error);
                    else if (error.Contains("Password"))
                        ModelState.AddModelError("vm.Password", error);
                }
                return BadRequest(ModelState);
            }
            DbContext.SteamBindingTokens.Remove(steamBindingToken);

            // Auto login
            await SignInManager.SignInAsync(user, true, true);
            var loginLog = new LoginLog
            {
                Ip = OwinContext.Request.RemoteIpAddress,
                UserId = user.Id
            };
            DbContext.LoginLogs.Add(loginLog);
            await DbContext.SaveChangesAsync();
            return Created($"user/{user.Id}", new LoginLogDTO(loginLog));
        }

        /// <summary>
        /// 修改用户设置
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="vm">用户相关属性</param>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权编辑指定用户")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Put(string id, UserPutVM vm)
        {
            if (User.Identity.GetUserId() != id)
                return Unauthorized();

            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByIdAsync(id);

            if (vm.NewPassword != null || vm.LockoutEnabled != null)
            {
                if (vm.Password == null)
                {
                    ModelState.AddModelError("vm.Password", "Password cannot be empty.");
                    return BadRequest(ModelState);
                }

                var geetest = new Geetest();
                if (vm.GeetestChallenge == null || vm.GeetestSeccode == null || vm.GeetestValidate == null ||
                    !await geetest.ValidateAsync(vm.GeetestChallenge, vm.GeetestSeccode, vm.GeetestValidate))
                {
                    ModelState.AddModelError("authCode", "true");
                    return BadRequest(ModelState);
                }

                if (vm.NewPassword != null)
                {
                    var resultPassword = await UserManager.ChangePasswordAsync(id, vm.Password, vm.NewPassword);
                    if (!resultPassword.Succeeded)
                    {
                        foreach (var error in resultPassword.Errors)
                        {
                            if (error.Contains("Incorrect password"))
                                ModelState.AddModelError("vm.Password", "Password is not correct.");
                            else
                                ModelState.AddModelError("vm.NewPassword", error);
                        }
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    if (!await UserManager.CheckPasswordAsync(user, vm.Password))
                    {
                        ModelState.AddModelError("vm.Password", "Password is not correct.");
                        return BadRequest(ModelState);
                    }
                }
            }

            vm.CopyToUser(user);
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Contains("Email"))
                        ModelState.AddModelError("vm.Email", error);
                    else if (error.Contains("GamerTag"))
                        ModelState.AddModelError("vm.GamerTag", error);
                }
                return BadRequest(ModelState);
            }
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