using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Drawing;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Controllers.Point;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;


namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        /// 更新用户资料
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "你是从火星来的吗？")]
        public async Task<IHttpActionResult> UpdateOne(string id,[NotNull] UserUpdateOneRequestDto requestDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            //todo 昵称
            //
            if (requestDto.Email != null)
                user.Email = requestDto.Email;
            if (requestDto.GamerTag != null)
                user.GamerTag = requestDto.GamerTag;
            if (requestDto.HeaderImage != null && Helpers.IsTrustedUrl(requestDto.HeaderImage))
                user.HeaderImage = requestDto.HeaderImage;
            if (requestDto.AvatarImage != null && Helpers.IsTrustedUrl(requestDto.AvatarImage))
                user.AvatarImage = requestDto.AvatarImage;
            if (requestDto.ThemeColor != null)
            {
                try
                {
                    user.ThemeColor = ColorTranslator.ToHtml(ColorTranslator.FromHtml(requestDto.ThemeColor));
                }
                catch (Exception)
                {
                    user.ThemeColor = string.Empty;
                }
                
            }
            if (requestDto.LightThemeColor != null)
            {
                try
                {
                    user.LightThemeColor = ColorTranslator.ToHtml(ColorTranslator.FromHtml(requestDto.LightThemeColor));
                }
                catch (Exception)
                {
                    user.LightThemeColor = string.Empty;
                }
            }
            if (requestDto.NewPassword != null || requestDto.LockoutEnabled != null)
            {
                //todo password 
            }
            if(requestDto.LockoutEnabled !=null)
                user.LockoutEnabled = requestDto.LockoutEnabled.Value;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var error = updateResult.Errors.First();
                string errorName;
                switch (error)
                {
                    case Errors.InvalidEmail:
                        errorName = nameof(requestDto.Email);
                        break;

                    case Errors.UserNameInvalidCharacter:
                    case Errors.UserNameInvalidLength:
                    case Errors.UserNameUsed:
                        errorName = nameof(requestDto.UserName);
                        break;
                    case Errors.EmailUsed:
                        errorName = nameof(requestDto.Email);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return this.BadRequest(nameof(requestDto), errorName, error);
            }

            return Ok();
        }


        /// <summary>
        /// User UpdateOne request DTO
        /// </summary>
        public class UserUpdateOneRequestDto
        {
            /// <summary>
            /// 昵称
            /// </summary>
            [MaxLength(16)]
            public string UserName { get; set; }


            /// <summary>
            /// 电邮地址
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 玩家标签
            /// </summary>
            [MaxLength(100)]
            public string GamerTag { get; set; }


            /// <summary>
            /// 页眉照片
            /// </summary>
            [MaxLength(256)]
            public string HeaderImage { get; set; }

            /// <summary>
            /// 头像图表
            /// </summary>
            [MaxLength(256)]
            public string AvatarImage { get; set; }

            /// <summary>
            /// 主题色
            /// </summary>
            [MaxLength(7)]
            public string ThemeColor { get; set; }

            /// <summary>
            /// 轻主题色
            /// </summary>
            [MaxLength(7)]
            public string LightThemeColor { get; set; }

            /// <summary>
            /// 口令
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// 口令
            /// </summary>
            public string NewPassword { get; set; }

            /// <summary>
            /// 登录保护
            /// </summary>
            public bool? LockoutEnabled { get; set; }

        }


    }
}
