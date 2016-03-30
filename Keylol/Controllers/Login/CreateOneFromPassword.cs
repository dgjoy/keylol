using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
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
        /// <param name="requestDto">登录所需相关属性</param>
        [AllowAnonymous]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (LoginLogDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOneFromPassword(LoginCreateOneFromPasswordRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var geetest = new Geetest();
            if (!await geetest.ValidateAsync(requestDto.GeetestChallenge, requestDto.GeetestSeccode, requestDto.GeetestValidate))
            {
                ModelState.AddModelError("authCode", "true");
                return BadRequest(ModelState);
            }
            var user = Regex.IsMatch(requestDto.EmailOrIdCode, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}$",
                RegexOptions.IgnoreCase)
                ? await UserManager.FindByEmailAsync(requestDto.EmailOrIdCode)
                : await DbContext.Users.SingleOrDefaultAsync(keylolUser => keylolUser.IdCode == requestDto.EmailOrIdCode);
            if (user == null)
            {
                ModelState.AddModelError("vm.EmailOrIdCode", "User doesn't exist.");
                return BadRequest(ModelState);
            }
            var result = await SignInManager.PasswordSignInAsync(user.UserName, requestDto.Password, true, true);
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
                    return Created($"login/{loginLog.Id}", new LoginLogDto(loginLog));

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
        /// 请求 DTO
        /// </summary>
        public class LoginCreateOneFromPasswordRequestDto
        {
            /// <summary>
            /// Email 或者识别码
            /// </summary>
            [Required]
            public string EmailOrIdCode { get; set; }

            /// <summary>
            /// 密码
            /// </summary>
            [Required]
            public string Password { get; set; }

            /// <summary>
            /// 极验 Chanllenge
            /// </summary>
            [Required]
            public string GeetestChallenge { get; set; }

            /// <summary>
            /// 极验 Seccode
            /// </summary>
            [Required]
            public string GeetestSeccode { get; set; }

            /// <summary>
            /// 极验 Validate
            /// </summary>
            [Required]
            public string GeetestValidate { get; set; }
        }
    }
}