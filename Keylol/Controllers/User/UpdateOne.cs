using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Utilities;
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
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] UserUpdateOneRequestDto requestDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // TODO: 昵称
            
            if (requestDto.Email != null)
                user.Email = requestDto.Email;
            if (requestDto.GamerTag != null)
                user.GamerTag = requestDto.GamerTag;
            if (requestDto.HeaderImage != null)
                user.HeaderImage = requestDto.HeaderImage;
            if (requestDto.AvatarImage != null)
                user.AvatarImage = requestDto.AvatarImage;
            if (requestDto.ThemeColor != null)
            {
                user.ThemeColor = requestDto.ThemeColor;
            }
            if (requestDto.LightThemeColor != null)
            {
                user.LightThemeColor = requestDto.LightThemeColor;
            }
            if (requestDto.NewPassword != null)
            {
                if (requestDto.Password == null || !await _userManager.CheckPasswordAsync(user, requestDto.Password))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Password), Errors.Invalid);

                var resultPassword =
                    await _userManager.ChangePasswordAsync(user.Id, requestDto.Password, requestDto.NewPassword);
                if (!resultPassword.Succeeded)
                {
                    var error = resultPassword.Errors.First();
                    switch (error)
                    {
                        case Errors.PasswordAllWhitespace:
                        case Errors.PasswordTooShort:
                            return this.BadRequest(nameof(requestDto), nameof(requestDto.NewPassword), error);
                        default:
                            return this.BadRequest(nameof(requestDto), nameof(requestDto.Password),
                                Errors.InvalidPassword);
                    }
                }
            }

            if (requestDto.LockoutEnabled != null)
                user.LockoutEnabled = requestDto.LockoutEnabled.Value;

            if (requestDto.OpenInNewWindow != null)
                user.OpenInNewWindow = requestDto.OpenInNewWindow.Value;

            if (requestDto.PreferredPointName != null)
            {
                user.PreferredPointName = requestDto.PreferredPointName.Value
                    ? PreferredPointName.English
                    : PreferredPointName.Chinese;
            }

            if (requestDto.NotifyOnArtivleReplied != null)
                user.NotifyOnArtivleReplied = requestDto.NotifyOnArtivleReplied.Value;

            if (requestDto.NotifyOnCommentReplied != null)
                user.NotifyOnCommentReplied = requestDto.NotifyOnCommentReplied.Value;

            if (requestDto.NotifyOnArtivleLiked != null)
                user.NotifyOnArtivleLiked = requestDto.NotifyOnArtivleLiked.Value;

            if (requestDto.NotifyOnCommentLiked != null)
                user.NotifyOnCommentLiked = requestDto.NotifyOnCommentLiked.Value;

            if (requestDto.SteamNotifyOnArticleReplied != null)
                user.SteamNotifyOnArticleReplied = requestDto.SteamNotifyOnArticleReplied.Value;

            if (requestDto.SteamNotifyOnCommentReplied != null)
                user.SteamNotifyOnCommentReplied = requestDto.SteamNotifyOnCommentReplied.Value;

            if (requestDto.SteamNotifyOnArticleLiked != null)
                user.SteamNotifyOnArticleLiked = requestDto.SteamNotifyOnArticleLiked.Value;

            if (requestDto.SteamNotifyOnCommentLiked != null)
                user.SteamNotifyOnCommentLiked = requestDto.SteamNotifyOnCommentLiked.Value;

            if (requestDto.SteamNotifyOnSpotlighted != null)
                user.SteamNotifyOnSpotlighted = requestDto.SteamNotifyOnSpotlighted.Value;

            if (requestDto.SteamNotifyOnMissive != null)
                user.SteamNotifyOnMissive = requestDto.SteamNotifyOnMissive.Value;


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

                    case Errors.InvalidThemeColor:
                        errorName = nameof(requestDto.ThemeColor);
                        break;

                    case Errors.InvalidLightThemeColor:
                        errorName = nameof(requestDto.LightThemeColor);
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
            public string UserName { get; set; }


            /// <summary>
            /// 电邮地址
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 玩家标签
            /// </summary>
            public string GamerTag { get; set; }


            /// <summary>
            /// 页眉照片
            /// </summary>
            public string HeaderImage { get; set; }

            /// <summary>
            /// 头像图表
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            /// 主题色
            /// </summary>
            public string ThemeColor { get; set; }

            /// <summary>
            /// 轻主题色
            /// </summary>
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

            /// <summary>
            /// 新窗口打开
            /// </summary>
            public bool? OpenInNewWindow { get; set; }

            /// <summary>
            /// 主选外语
            /// </summary>
            public bool? PreferredPointName { get; set; }

            /// <summary>
            /// 邮局，文章收到评论
            /// </summary>
            public bool? NotifyOnArtivleReplied { get; set; }

            /// <summary>
            /// 邮局，评论被回复
            /// </summary>
            public bool? NotifyOnCommentReplied { get; set; }

            /// <summary>
            /// 邮局，文章获得认可
            /// </summary>
            public bool? NotifyOnArtivleLiked { get; set; }

            /// <summary>
            /// 邮局，评论获得认可
            /// </summary>
            public bool? NotifyOnCommentLiked { get; set; }

            /// <summary>
            /// Steam机器人，文章收到评论
            /// </summary>
            public bool? SteamNotifyOnArticleReplied { get; set; }

            /// <summary>
            /// Steam机器人，评论被回复
            /// </summary>
            public bool? SteamNotifyOnCommentReplied { get; set; }

            /// <summary>
            /// Steam机器人，文章获得认可
            /// </summary>
            public bool? SteamNotifyOnArticleLiked { get; set; }

            /// <summary>
            /// Steam机器人，评论获得认可
            /// </summary>
            public bool? SteamNotifyOnCommentLiked { get; set; }

            /// <summary>
            /// Steam机器人，萃选推选
            /// </summary>
            public bool? SteamNotifyOnSpotlighted { get; set; }

            /// <summary>
            /// Steam机器人，系统公函
            /// </summary>
            public bool? SteamNotifyOnMissive { get; set; }
        }
    }
}