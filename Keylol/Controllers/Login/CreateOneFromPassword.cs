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

namespace Keylol.Controllers.Login
{
    public partial class LoginController
    {
        /// <summary>
        ///     使用密码登录
        /// </summary>
        /// <param name="vm">登录所需相关属性</param>
        [AllowAnonymous]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (LoginLogDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOneFromPassword(LoginVM vm)
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
    }
}