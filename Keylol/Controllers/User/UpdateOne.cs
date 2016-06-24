using System.Linq;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
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
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权编辑该用户资料")]
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] UserUpdateOneRequestDto requestDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            if (currentUserId != user.Id && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            if (requestDto.Email != null)
                user.Email = requestDto.Email;

            if (requestDto.GamerTag != null)
                user.GamerTag = requestDto.GamerTag;

            if (requestDto.HeaderImage != null)
                user.HeaderImage = requestDto.HeaderImage;

            if (requestDto.AvatarImage != null)
                user.AvatarImage = requestDto.AvatarImage;

            if (requestDto.ThemeColor != null)
                user.ThemeColor = requestDto.ThemeColor;

            if (requestDto.LightThemeColor != null)
                user.LightThemeColor = requestDto.LightThemeColor;

            if (requestDto.NewPassword != null)
            {
                if (currentUserId == user.Id &&
                    (requestDto.Password == null || !await _userManager.CheckPasswordAsync(user, requestDto.Password)))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Password), Errors.Invalid);

                var passwordResult = await _userManager.ChangePasswordAsync(user, requestDto.NewPassword, false);
                if (!passwordResult.Succeeded)
                {
                    var passwordError = passwordResult.Errors.First();
                    string errorPropertyName;
                    switch (passwordError)
                    {
                        case Errors.PasswordAllWhitespace:
                        case Errors.PasswordTooShort:
                            errorPropertyName = nameof(requestDto.NewPassword);
                            break;

                        default:
                            errorPropertyName = nameof(requestDto.Password);
                            break;
                    }
                    return this.BadRequest(nameof(requestDto), errorPropertyName, passwordError);
                }
            }

            if (requestDto.LockoutEnabled != null)
                user.LockoutEnabled = requestDto.LockoutEnabled.Value;

            if (requestDto.OpenInNewWindow != null)
                user.OpenInNewWindow = requestDto.OpenInNewWindow.Value;

            if (requestDto.UseEnglishPointName != null)
                user.PreferredPointName = requestDto.UseEnglishPointName.Value
                    ? PreferredPointName.English
                    : PreferredPointName.Chinese;

            if (requestDto.NotifyOnArticleReplied != null)
                user.NotifyOnArticleReplied = requestDto.NotifyOnArticleReplied.Value;

            if (requestDto.NotifyOnCommentReplied != null)
                user.NotifyOnCommentReplied = requestDto.NotifyOnCommentReplied.Value;

            if (requestDto.NotifyOnActivityReplied != null)
                user.NotifyOnActivityReplied = requestDto.NotifyOnActivityReplied.Value;

            if (requestDto.NotifyOnArticleLiked != null)
                user.NotifyOnArticleLiked = requestDto.NotifyOnArticleLiked.Value;

            if (requestDto.NotifyOnCommentLiked != null)
                user.NotifyOnCommentLiked = requestDto.NotifyOnCommentLiked.Value;

            if (requestDto.NotifyOnActivityLiked != null)
                user.NotifyOnActivityLiked = requestDto.NotifyOnActivityLiked.Value;

            if (requestDto.NotifyOnSubscribed != null)
                user.NotifyOnSubscribed = requestDto.NotifyOnSubscribed.Value;

            if (requestDto.SteamNotifyOnArticleReplied != null)
                user.SteamNotifyOnArticleReplied = requestDto.SteamNotifyOnArticleReplied.Value;

            if (requestDto.SteamNotifyOnCommentReplied != null)
                user.SteamNotifyOnCommentReplied = requestDto.SteamNotifyOnCommentReplied.Value;

            if (requestDto.SteamNotifyOnActivityReplied != null)
                user.SteamNotifyOnActivityReplied = requestDto.SteamNotifyOnActivityReplied.Value;

            if (requestDto.SteamNotifyOnArticleLiked != null)
                user.SteamNotifyOnArticleLiked = requestDto.SteamNotifyOnArticleLiked.Value;

            if (requestDto.SteamNotifyOnCommentLiked != null)
                user.SteamNotifyOnCommentLiked = requestDto.SteamNotifyOnCommentLiked.Value;

            if (requestDto.SteamNotifyOnActivityLiked != null)
                user.SteamNotifyOnActivityLiked = requestDto.SteamNotifyOnActivityLiked.Value;

            if (requestDto.SteamNotifyOnSubscribed != null)
                user.SteamNotifyOnSubscribed = requestDto.SteamNotifyOnSubscribed.Value;

            if (requestDto.SteamNotifyOnSpotlighted != null)
                user.SteamNotifyOnSpotlighted = requestDto.SteamNotifyOnSpotlighted.Value;

            if (requestDto.SteamNotifyOnMissive != null)
                user.SteamNotifyOnMissive = requestDto.SteamNotifyOnMissive.Value;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded) return Ok();

            var updateError = updateResult.Errors.First();
            string propertyName;
            switch (updateError)
            {
                case Errors.UserNameInvalidCharacter:
                case Errors.UserNameInvalidLength:
                case Errors.UserNameUsed:
                    propertyName = nameof(requestDto.UserName);
                    break;

                case Errors.InvalidEmail:
                case Errors.EmailUsed:
                    propertyName = nameof(requestDto.Email);
                    break;

                case Errors.GamerTagInvalidLength:
                    propertyName = nameof(requestDto.GamerTag);
                    break;

                case Errors.AvatarImageUntrusted:
                    propertyName = nameof(requestDto.AvatarImage);
                    break;

                case Errors.HeaderImageUntrusted:
                    propertyName = nameof(requestDto.HeaderImage);
                    break;

                case Errors.InvalidThemeColor:
                    propertyName = nameof(requestDto.ThemeColor);
                    break;

                case Errors.InvalidLightThemeColor:
                    propertyName = nameof(requestDto.LightThemeColor);
                    break;

                default:
                    return this.BadRequest(nameof(requestDto), updateError);
            }
            return this.BadRequest(nameof(requestDto), propertyName, updateError);
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
            /// 邮箱
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 玩家标签
            /// </summary>
            public string GamerTag { get; set; }

            /// <summary>
            /// 页眉图片
            /// </summary>
            public string HeaderImage { get; set; }

            /// <summary>
            /// 头像图标
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
            /// 原口令
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// 新口令
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
            public bool? UseEnglishPointName { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 文章收到评论
            /// </summary>
            public bool? NotifyOnArticleReplied { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 评论被回复
            /// </summary>
            public bool? NotifyOnCommentReplied { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 动态收到评论
            /// </summary>
            public bool? NotifyOnActivityReplied { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 文章获得认可
            /// </summary>
            public bool? NotifyOnArticleLiked { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 评论获得认可
            /// </summary>
            public bool? NotifyOnCommentLiked { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 动态获得认可
            /// </summary>
            public bool? NotifyOnActivityLiked { get; set; }

            /// <summary>
            /// 邮政中心提醒 - 新听众
            /// </summary>
            public bool? NotifyOnSubscribed { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 文章收到评论
            /// </summary>
            public bool? SteamNotifyOnArticleReplied { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 评论被回复
            /// </summary>
            public bool? SteamNotifyOnCommentReplied { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 动态收到评论
            /// </summary>
            public bool? SteamNotifyOnActivityReplied { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 文章获得认可
            /// </summary>
            public bool? SteamNotifyOnArticleLiked { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 评论获得认可
            /// </summary>
            public bool? SteamNotifyOnCommentLiked { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 动态获得认可
            /// </summary>
            public bool? SteamNotifyOnActivityLiked { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 新听众
            /// </summary>
            public bool? SteamNotifyOnSubscribed { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 萃选推送
            /// </summary>
            public bool? SteamNotifyOnSpotlighted { get; set; }

            /// <summary>
            /// Steam 机器人提醒 - 系统公函
            /// </summary>
            public bool? SteamNotifyOnMissive { get; set; }
        }
    }
}