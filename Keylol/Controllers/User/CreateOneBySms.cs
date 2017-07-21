using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///    通过 sms 注册一个新用户
        /// </summary>
        /// <param name="bySmsRequestDto">用户相关属性</param>
        [AllowAnonymous]
        [Route("user/sms")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneBySms([NotNull] UserCreateOneBySmsRequestDto bySmsRequestDto)
        {
            // 检查手机是否合法
            if (bySmsRequestDto.PhoneNumber == null || !new PhoneAttribute().IsValid(bySmsRequestDto.PhoneNumber))
            {
                return BadRequest(Errors.InvalidPhoneNumber);
            }

            // 检查手机是否已经注册 
            if (bySmsRequestDto.PhoneNumber != null &&
                await _userManager.FindByPhoneNumberAsync(bySmsRequestDto.PhoneNumber) != null)
                return BadRequest(Errors.PhoneNumberUsed);


            if (bySmsRequestDto.Email != null && (!new EmailAddressAttribute().IsValid(bySmsRequestDto.Email) ||
                                                  await _userManager.FindByEmailAsync(bySmsRequestDto.Email) != null))
                bySmsRequestDto.Email = null;

            var user = new KeylolUser
            {
                IdCode = bySmsRequestDto.IdCode,
                UserName = bySmsRequestDto.UserName,
                Email = bySmsRequestDto.Email,
                PhoneNumber = bySmsRequestDto.PhoneNumber,
                RegisterIp = _owinContext.Request.RemoteIpAddress,
                SteamBindingTime = DateTime.Now
            };

            if (bySmsRequestDto.AvatarImage != null)
                user.AvatarImage = bySmsRequestDto.AvatarImage;

            if (bySmsRequestDto.SteamProfileName != null)
                user.SteamProfileName = bySmsRequestDto.SteamProfileName;

            var result = await _userManager.CreateAsync(user, bySmsRequestDto.Password);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                string propertyName;
                switch (error)
                {
                    case Errors.InvalidIdCode:
                    case Errors.IdCodeReserved:
                    case Errors.IdCodeUsed:
                        propertyName = nameof(bySmsRequestDto.IdCode);
                        break;

                    case Errors.UserNameInvalidCharacter:
                    case Errors.UserNameInvalidLength:
                    case Errors.UserNameUsed:
                        propertyName = nameof(bySmsRequestDto.UserName);
                        break;

                    case Errors.InvalidEmail:
                        propertyName = nameof(bySmsRequestDto.Email);
                        break;

                    case Errors.AvatarImageUntrusted:
                        propertyName = nameof(bySmsRequestDto.AvatarImage);
                        break;

                    case Errors.PasswordAllWhitespace:
                    case Errors.PasswordTooShort:
                        propertyName = nameof(bySmsRequestDto.Password);
                        break;

                    default:
                        return this.BadRequest(nameof(bySmsRequestDto), error);
                }
                return this.BadRequest(nameof(bySmsRequestDto), propertyName, error);
            }

            await _userManager.AddLoginAsync(user.Id,
                new UserLoginInfo(KeylolLoginProviders.Sms, user.PhoneNumber));
            await _dbContext.SaveChangesAsync();

            if (bySmsRequestDto.SteamCnUserName != null)
            {
                var steamCnUser =
                    await SteamCnProvider.UserLoginAsync(bySmsRequestDto.SteamCnUserName, bySmsRequestDto.SteamCnPassword, false);
                if (steamCnUser != null && steamCnUser.Uid > 0 &&
                    await _userManager.FindAsync(new UserLoginInfo(KeylolLoginProviders.SteamCn,
                        steamCnUser.Uid.ToString())) == null)
                {
                    await _userManager.AddLoginAsync(user.Id, new UserLoginInfo(KeylolLoginProviders.SteamCn,
                        steamCnUser.Uid.ToString()));
                    user.SteamCnUserName = steamCnUser.UserName;
                    user.SteamCnBindingTime = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                }
            }

            await _coupon.UpdateAsync(user, CouponEvent.新注册);

            var inviterText = string.Empty;
            if (bySmsRequestDto.InviterIdCode != null)
            {
                var inviter = await _userManager.FindByIdCodeAsync(bySmsRequestDto.InviterIdCode);
                if (inviter != null)
                {
                    user.InviterId = inviter.Id;
                    await _dbContext.SaveChangesAsync();
                    await _coupon.UpdateAsync(inviter, CouponEvent.邀请注册, new { UserId = user.Id });
                    await _coupon.UpdateAsync(user, CouponEvent.应邀注册, new { InviterId = user.Id });
                    inviterText = $"邀请人：{inviter.UserName} ({inviter.IdCode})\n";
                }
            }

            AutoSubscribe(user.Id);

            var operatorRoleId = (await _roleManager.FindByNameAsync(KeylolRoles.Operator)).Id;
            foreach (var @operator in await _dbContext.Users
                .Where(u => u.Roles.Any(r => r.RoleId == operatorRoleId)).ToListAsync())
            {
                await _userManager.SendSteamChatMessageAsync(@operator,
                    $"[新用户注册 {user.RegisterTime}]\n#{user.Sid} {user.UserName}\nSteam 昵称：{user.SteamProfileName}\nIP：{user.RegisterIp}\n{inviterText}https://www.keylol.com/user/{user.IdCode}");
            }

            return Ok(await _oneTimeToken.Generate(user.Id, TimeSpan.FromMinutes(1), OneTimeTokenPurpose.UserLogin));
        }


        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class UserCreateOneBySmsRequestDto
        {
            /// <summary>
            ///     识别码
            /// </summary>
            [Utilities.Required]
            public string IdCode { get; set; }

            /// <summary>
            ///     昵称（用户名）
            /// </summary>
            [Utilities.Required]
            public string UserName { get; set; }

            /// <summary>
            ///     口令
            /// </summary>
            [Utilities.Required]
            public string Password { get; set; }

            /// <summary>
            ///     头像
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            /// 邮箱
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 手机
            /// </summary>
            [Utilities.Required]
            public string PhoneNumber { get; set; }

            /// <summary>
            /// SteamCN 用户名
            /// </summary>
            public string SteamCnUserName { get; set; }

            /// <summary>
            /// SteamCN 密码
            /// </summary>
            public string SteamCnPassword { get; set; }

            /// <summary>
            ///     Steam 玩家昵称
            /// </summary>
            public string SteamProfileName { get; set; }

            /// <summary>
            ///     邀请人识别码
            /// </summary>
            public string InviterIdCode { get; set; }
        }
    }

}