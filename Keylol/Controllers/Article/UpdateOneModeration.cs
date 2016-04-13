using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Services;
using Keylol.Services.Contracts;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     更新文章的封存、退稿、萃选或警告状态
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}/moderation")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这篇文章")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            ArticleUpdateOneModerationRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("requestDto", "Invalid request DTO.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await DbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var operatorStaffClaim = await UserManager.GetStaffClaimAsync(operatorId);

            if (operatorStaffClaim != StaffClaim.Operator)
            {
                switch (requestDto.Property)
                {
                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                        if (article.PrincipalId != operatorId)
                            return Unauthorized();
                        break;
                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                        if (article.PrincipalId != operatorId || requestDto.Value)
                            return Unauthorized();
                        break;

                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Warned:
                        break;
                    default:
                        return Unauthorized();
                }
            }

            if (!Enum.IsDefined(typeof (ArticleUpdateOneModerationRequestDto.ArticleProperty), requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof (Models.Article).GetProperty(requestDto.Property.ToString());
            if (requestDto.Property == ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived)
            {
                if (article.Archived != ArchivedState.None == requestDto.Value)
                {
                    ModelState.AddModelError("requestDto.Value", "文章已经处于目标状态");
                    return BadRequest(ModelState);
                }
                if (operatorStaffClaim == StaffClaim.Operator)
                {
                    article.Archived = requestDto.Value ? ArchivedState.Operator : ArchivedState.None;
                }
                else
                {
                    if (article.Archived == ArchivedState.Operator)
                        return Unauthorized();
                    article.Archived = requestDto.Value ? ArchivedState.User : ArchivedState.None;
                }
            }
            else if (requestDto.Property == ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight)
            {
                if (article.SpotlightTime != null == requestDto.Value)
                {
                    ModelState.AddModelError("requestDto.Value", "文章已经处于目标状态");
                    return BadRequest(ModelState);
                }
                if (requestDto.Value)
                    article.SpotlightTime = DateTime.Now;
                else
                    article.SpotlightTime = null;
            }
            else
            {
                if ((bool) propertyInfo.GetValue(article) == requestDto.Value)
                {
                    ModelState.AddModelError("requestDto.Value", "文章已经处于目标状态");
                    return BadRequest(ModelState);
                }
                propertyInfo.SetValue(article, requestDto.Value);
            }
            if (operatorStaffClaim == StaffClaim.Operator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = DbContext.Messages.Create();
                missive.OperatorId = operatorId;
                missive.Receiver = article.Principal.User;
                missive.ArticleId = article.Id;
                string steamNotityText = null;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                            missive.Type = MessageType.ArticleArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"文章《{article.Title}》已被封存，封存后该文章的内容和所有评论会被隐藏，同时这篇文章不会再显示于任何信息轨道上。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.Rejection;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"文章《{article.Title}》已被退稿，不会再出现于其他用户或据点的讯息轨道上，这篇文章后续的投稿也将被自动回绝。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                            missive.Type = MessageType.Spotlight;
                            steamNotityText =
                                $"感谢你对其乐社区质量的认可与贡献！你的文章《{article.Title}》已被推荐为萃选文章，此文章将会从此刻开始展示在全站的「萃选文章」栏目中 14 天。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Warned:
                            missive.Type = MessageType.ArticleWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"文章《{article.Title}》已被警告，若在 30 天之内收到两次警告，你的账户将被自动停权 14 天。";
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                            missive.Type = MessageType.ArticleArchiveCancel;
                            steamNotityText = $"文章《{article.Title}》的封存已被撤销，该文章的内容和所有评论已重新公开，讯息轨道将不再隐藏这篇文章。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.RejectionCancel;
                            steamNotityText = $"文章《{article.Title}》的退稿限制已被撤销，其他用户首页的讯息轨道将不再隐藏这篇文章，后续的投稿也不再会被其他据点回绝。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                            missive.Type = MessageType.SpotlightCancel;
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Warned:
                            missive.Type = MessageType.ArticleWarningCancel;
                            steamNotityText = $"文章《{article.Title}》的警告已被撤销，之前的警告将不再纳入停权计数器的考量中，除非你的账户已经因收到警告而被自动停权。";
                            break;
                    }
                }
                DbContext.Messages.Add(missive);

                // Steam 通知
                SteamBotCoordinator botCoordinator;
                if (!string.IsNullOrEmpty(steamNotityText) && missive.Receiver.SteamBot.SessionId != null &&
                    SteamBotCoordinator.Sessions.TryGetValue(missive.Receiver.SteamBot.SessionId, out botCoordinator))
                {
                    botCoordinator.Client.SendMessage(missive.Receiver.SteamBotId, missive.Receiver.SteamId,
                        steamNotityText);
                }
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }

    /// <summary>
    ///     封存、退稿、萃选、警告请求 DTO
    /// </summary>
    public class ArticleUpdateOneModerationRequestDto
    {
        /// <summary>
        ///     文章属性
        /// </summary>
        public enum ArticleProperty
        {
            /// <summary>
            ///     封存状态
            /// </summary>
            Archived,

            /// <summary>
            ///     退稿状态
            /// </summary>
            Rejected,

            /// <summary>
            ///     萃选状态
            /// </summary>
            Spotlight,

            /// <summary>
            ///     警告状态
            /// </summary>
            Warned
        }

        /// <summary>
        ///     要操作的文章属性
        /// </summary>
        public ArticleProperty Property { get; set; }

        /// <summary>
        ///     文章属性的新值
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        ///     操作理由
        /// </summary>
        public List<int> Reasons { get; set; }

        /// <summary>
        ///     是否通知作者
        /// </summary>
        public bool? NotifyAuthor { get; set; }
    }
}