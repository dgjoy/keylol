using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
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
        public async Task<IHttpActionResult> CreateOne([NotNull] UserCreateOneRequestDto requestDto)
        {
            if (!await _geetest.ValidateAsync(requestDto.GeetestChallenge,
                requestDto.GeetestSeccode,
                requestDto.GeetestValidate))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.GeetestSeccode), Errors.Invalid);

            var steamBindingToken = await _dbContext.SteamBindingTokens.FindAsync(requestDto.SteamBindingTokenId);

            if (steamBindingToken == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamBindingTokenId), Errors.Invalid);

            var user = new KeylolUser
            {
                IdCode = requestDto.IdCode,
                UserName = requestDto.UserName,
                RegisterIp = _owinContext.Request.RemoteIpAddress,
                AvatarImage = requestDto.AvatarImage,
                SteamBindingTime = DateTime.Now,
                SteamId = steamBindingToken.SteamId,
                SteamProfileName = requestDto.SteamProfileName,
                SteamBotId = steamBindingToken.BotId
            };

            var result = await _userManager.CreateAsync(user, requestDto.Password);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                string propertyName;
                switch (error)
                {
                    case Errors.InvalidIdCode:
                    case Errors.IdCodeUsed:
                        propertyName = nameof(requestDto.IdCode);
                        break;

                    case Errors.UserNameInvalidLength:
                    case Errors.UserNameInvalidCharacter:
                    case Errors.UserNameUsed:
                        propertyName = nameof(requestDto.UserName);
                        break;

                    case Errors.AvatarImageUntrusted:
                        propertyName = nameof(requestDto.AvatarImage);
                        break;

                    case Errors.SteamAccountBound:
                        propertyName = nameof(requestDto.SteamBindingTokenId);
                        break;

                    case Errors.PasswordAllWhitespace:
                    case Errors.PasswordTooShort:
                        propertyName = nameof(requestDto.Password);
                        break;

                    default:
                        propertyName = nameof(requestDto.GeetestSeccode);
                        break;
                }
                return this.BadRequest(nameof(requestDto), propertyName, error);
            }

            _dbContext.SteamBindingTokens.Remove(steamBindingToken);
            await _dbContext.SaveChangesAsync();

            await _coupon.Update(user, CouponEvent.新注册);

            // 邀请人
            if (requestDto.Inviter != null)
            {
                var inviterIdCode = requestDto.Inviter;
                var inviter = await _userManager.FindByIdCodeAsync(inviterIdCode);
                if (inviter != null)
                {
                    user.InviterId = inviter.Id;
                    await _dbContext.SaveChangesAsync();
                    await _coupon.Update(inviter, CouponEvent.邀请注册, new {UserId = user.Id});
                    await _coupon.Update(user, CouponEvent.应邀注册, new {InviterId = user.Id});
                }
            }

            return Created($"user/{user.Id}", _oneTimeToken.Generate(user.Id, TimeSpan.FromMinutes(1)));
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