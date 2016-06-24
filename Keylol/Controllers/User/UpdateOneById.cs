using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     修改当前登录用户的设置
        /// </summary>
        /// <param name="requestDto">用户相关属性</param>
        [Route("current")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权编辑指定用户")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdateOneById([NotNull] UserUpdateOneByIdRequestDto requestDto)
        {
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            if (requestDto.NewPassword != null || requestDto.LockoutEnabled != null)
            {
                if (requestDto.Password == null)
                    this.BadRequest(nameof(requestDto), nameof(requestDto.Password), Errors.Invalid);

                if (!await _geetest.ValidateAsync(requestDto.GeetestChallenge,
                    requestDto.GeetestSeccode,
                    requestDto.GeetestValidate))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.GeetestSeccode), Errors.Invalid);

                if (requestDto.NewPassword != null)
                {
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
                                return this.BadRequest(nameof(requestDto), nameof(requestDto.Password), Errors.Invalid);
                        }
                    }
                }
                else
                {
                    if (!await _userManager.CheckPasswordAsync(user, requestDto.Password))
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.Password), Errors.Invalid);
                }
            }

            if (requestDto.GamerTag != null)
                user.GamerTag = requestDto.GamerTag;
            if (requestDto.Email != null)
                user.Email = requestDto.Email;
            if (requestDto.AvatarImage != null)
                user.AvatarImage = requestDto.AvatarImage;
            if (requestDto.ProfilePointBackgroundImage != null)
                user.ProfilePoint.BackgroundImage = requestDto.ProfilePointBackgroundImage;
            if (requestDto.LockoutEnabled != null)
                user.LockoutEnabled = requestDto.LockoutEnabled.Value;
            if (requestDto.SteamNotifyOnArticleReplied != null)
                user.SteamNotifyOnArticleReplied = requestDto.SteamNotifyOnArticleReplied.Value;
            if (requestDto.SteamNotifyOnArticleLiked != null)
                user.SteamNotifyOnArticleLiked = requestDto.SteamNotifyOnArticleLiked.Value;
            if (requestDto.SteamNotifyOnCommentReplied != null)
                user.SteamNotifyOnCommentReplied = requestDto.SteamNotifyOnCommentReplied.Value;
            if (requestDto.SteamNotifyOnCommentLiked != null)
                user.SteamNotifyOnCommentLiked = requestDto.SteamNotifyOnCommentLiked.Value;
            if (requestDto.AutoSubscribeEnabled != null)
                user.AutoSubscribeEnabled = requestDto.AutoSubscribeEnabled.Value;
            if (requestDto.AutoSubscribeDaySpan != null)
                user.AutoSubscribeDaySpan = requestDto.AutoSubscribeDaySpan.Value;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                string propertyName;
                switch (error)
                {
                    case Errors.AvatarImageUntrusted:
                        propertyName = nameof(requestDto.AvatarImage);
                        break;

                    case Errors.BackgroundImageUntrusted:
                        propertyName = nameof(requestDto.ProfilePointBackgroundImage);
                        break;

                    case Errors.GamerTagInvalidLength:
                        propertyName = nameof(requestDto.GamerTag);
                        break;

                    case Errors.InvalidEmail:
                        propertyName = nameof(requestDto.Email);
                        break;

                    default:
                        propertyName = nameof(requestDto.GeetestSeccode);
                        break;
                }
                return this.BadRequest(nameof(requestDto), propertyName, error);
            }
            return Ok();
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class UserUpdateOneByIdRequestDto
        {
            /// <summary>
            ///     玩家标签
            /// </summary>
            public string GamerTag { get; set; }

            /// <summary>
            ///     Email
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            ///     头像
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            ///     个人据点背景横幅
            /// </summary>
            public string ProfilePointBackgroundImage { get; set; }

            /// <summary>
            ///     旧密码
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            ///     新密码
            /// </summary>
            public string NewPassword { get; set; }

            /// <summary>
            ///     登录保护开关
            /// </summary>
            public bool? LockoutEnabled { get; set; }

            /// <summary>
            ///     极验 Chanllenge
            /// </summary>
            public string GeetestChallenge { get; set; }

            /// <summary>
            ///     极验 Seccode
            /// </summary>
            public string GeetestSeccode { get; set; }

            /// <summary>
            ///     极验 Validate
            /// </summary>
            public string GeetestValidate { get; set; }

            /// <summary>
            ///     Steam 通知开关：文章收到评论
            /// </summary>
            public bool? SteamNotifyOnArticleReplied { get; set; }

            /// <summary>
            ///     Steam 通知开关：评论收到回复
            /// </summary>
            public bool? SteamNotifyOnCommentReplied { get; set; }

            /// <summary>
            ///     Steam 通知开关：文章被认可
            /// </summary>
            public bool? SteamNotifyOnArticleLiked { get; set; }

            /// <summary>
            ///     Steam 通知开关：评论被认可
            /// </summary>
            public bool? SteamNotifyOnCommentLiked { get; set; }

            /// <summary>
            ///     同步订阅开关
            /// </summary>
            public bool? AutoSubscribeEnabled { get; set; }

            /// <summary>
            ///     同步订阅周期
            /// </summary>
            public int? AutoSubscribeDaySpan { get; set; }
        }
    }
}