using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     注册一个新用户
        /// </summary>
        /// <param name="requestDto">用户相关属性</param>
        [AllowAnonymous]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (LoginLogDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOne(UserCreateOneRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var geetest = new Geetest();
            if (
                !await
                    geetest.ValidateAsync(requestDto.GeetestChallenge, requestDto.GeetestSeccode,
                        requestDto.GeetestValidate))
            {
                ModelState.AddModelError("authCode", "true");
                return BadRequest(ModelState);
            }
            var steamBindingToken = await DbContext.SteamBindingTokens.FindAsync(requestDto.SteamBindingTokenId);
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
            if (!Regex.IsMatch(requestDto.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "Only 5 uppercase letters and digits are allowed in IdCode.");
                return BadRequest(ModelState);
            }

            if (await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == requestDto.IdCode) != null ||
                !IsIdCodeLegit(requestDto.IdCode))
            {
                ModelState.AddModelError("vm.IdCode", "IdCode is already used by others.");
                return BadRequest(ModelState);
            }
            if (await UserManager.FindByNameAsync(requestDto.UserName) != null)
            {
                ModelState.AddModelError("vm.UserName", "UserName is already used by others.");
                return BadRequest(ModelState);
            }
            if (!requestDto.AvatarImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }
            var user = new KeylolUser
            {
                IdCode = requestDto.IdCode,
                UserName = requestDto.UserName,
                RegisterIp = OwinContext.Request.RemoteIpAddress,
                AvatarImage = requestDto.AvatarImage,
                SteamBindingTime = DateTime.Now,
                SteamId = steamBindingToken.SteamId,
                SteamProfileName = requestDto.SteamProfileName,
                SteamBotId = steamBindingToken.BotId
            };

            var result = await UserManager.CreateAsync(user, requestDto.Password);
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
            await DbContext.SaveChangesAsync();

            await _coupon.Update(user.Id, CouponEvent.新注册);

            // 邀请人
            if (requestDto.Inviter != null)
            {
                var inviterIdCode = requestDto.Inviter;
                var inviter = await DbContext.Users.Where(u => u.IdCode == inviterIdCode).SingleOrDefaultAsync();
                if (inviter != null)
                {
                    await DbContext.Entry(user).ReloadAsync();
                    user.InviterId = inviter.Id;
                    await DbContext.SaveChangesAsync();
                    await _coupon.Update(inviter.Id, CouponEvent.邀请注册, new {UserId = user.Id});
                    await _coupon.Update(user.Id, CouponEvent.应邀注册, new {InviterId = user.Id});
                }
            }

            // Auto login
            await SignInManager.SignInAsync(user, true, true);
            var loginLog = new LoginLog
            {
                Ip = OwinContext.Request.RemoteIpAddress,
                UserId = user.Id
            };
            DbContext.LoginLogs.Add(loginLog);
            await DbContext.SaveChangesAsync();

            return Created($"user/{user.Id}", new LoginLogDto(loginLog));
        }

        private static bool IsIdCodeLegit(string idCode)
        {
            if (new[]
            {
                @"^([A-Z0-9])\1{4}$",
                @"^0000\d$",
                @"^\d0000$",
                @"^TEST.$",
                @"^.TEST$"
            }.Any(pattern => Regex.IsMatch(idCode, pattern)))
                return false;

            if (new[]
            {
                "12345",
                "54321",
                "ADMIN",
                "STAFF",
                "KEYLO",
                "KYLOL",
                "KEYLL",
                "VALVE",
                "STEAM",
                "CHINA",
                "JAPAN"
            }.Contains(idCode))
                return false;

            return true;
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class UserCreateOneRequestDto
        {
            /// <summary>
            ///     识别码
            /// </summary>
            [Required]
            public string IdCode { get; set; }

            /// <summary>
            ///     用户名
            /// </summary>
            [Required]
            public string UserName { get; set; }

            /// <summary>
            ///     密码
            /// </summary>
            [Required]
            public string Password { get; set; }

            /// <summary>
            ///     头像
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string AvatarImage { get; set; }

            /// <summary>
            ///     SteamBindingToken Id
            /// </summary>
            [Required]
            public string SteamBindingTokenId { get; set; }

            /// <summary>
            ///     Steam 玩家昵称
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string SteamProfileName { get; set; }

            /// <summary>
            ///     极验 Chanllenge
            /// </summary>
            [Required]
            public string GeetestChallenge { get; set; }

            /// <summary>
            ///     极验 Seccode
            /// </summary>
            [Required]
            public string GeetestSeccode { get; set; }

            /// <summary>
            ///     极验 Validate
            /// </summary>
            [Required]
            public string GeetestValidate { get; set; }

            /// <summary>
            ///     邀请人识别码
            /// </summary>
            public string Inviter { get; set; }
        }
    }
}