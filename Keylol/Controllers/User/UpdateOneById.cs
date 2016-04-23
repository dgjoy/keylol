using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     修改用户设置
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="requestDto">用户相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权编辑指定用户")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdateOneById(string id, UserUpdateOneByIdRequestDto requestDto)
        {
            if (User.Identity.GetUserId() != id)
                return Unauthorized();

            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!requestDto.AvatarImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            if (!requestDto.ProfilePointBackgroundImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.ProfilePointBackgroundImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);

            if (requestDto.NewPassword != null || requestDto.LockoutEnabled != null)
            {
                if (requestDto.Password == null)
                {
                    ModelState.AddModelError("vm.Password", "Password cannot be empty.");
                    return BadRequest(ModelState);
                }
                
                if (requestDto.GeetestChallenge == null || requestDto.GeetestSeccode == null ||
                    requestDto.GeetestValidate == null ||
                    !await
                        _geetest.ValidateAsync(requestDto.GeetestChallenge, requestDto.GeetestSeccode,
                            requestDto.GeetestValidate))
                {
                    ModelState.AddModelError("authCode", "true");
                    return BadRequest(ModelState);
                }

                if (requestDto.NewPassword != null)
                {
                    var resultPassword =
                        await _userManager.ChangePasswordAsync(id, requestDto.Password, requestDto.NewPassword);
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
                    if (!await _userManager.CheckPasswordAsync(user, requestDto.Password))
                    {
                        ModelState.AddModelError("vm.Password", "Password is not correct.");
                        return BadRequest(ModelState);
                    }
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