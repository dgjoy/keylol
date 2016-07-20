using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.States.PostOffice;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Activity
{
    public partial class ActivityController
    {
        /// <summary>
        ///     更新动态的封存、退稿或警告状态
        /// </summary>
        /// <param name="id">动态 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}/moderation")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定动态不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这则动态")]
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            [NotNull] ActivityUpdateOneModerationRequestDto requestDto)
        {
            var activity =
                await _dbContext.Activities.Include(a => a.Author).Where(a => a.Id == id).SingleOrDefaultAsync();
            if (activity == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var isKeylolOperator = User.IsInRole(KeylolRoles.Operator);

            if (!isKeylolOperator)
            {
                switch (requestDto.Property)
                {
                    case ActivityUpdateOneModerationRequestDto.ActivityProperty.Archived:
                        if (activity.AuthorId != operatorId)
                            return Unauthorized();
                        break;

                    default:
                        return Unauthorized();
                }
            }

            if (!Enum.IsDefined(typeof(ActivityUpdateOneModerationRequestDto.ActivityProperty), requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof(Models.Activity).GetProperty(requestDto.Property.ToString());
            if (requestDto.Property == ActivityUpdateOneModerationRequestDto.ActivityProperty.Archived)
            {
                if (activity.Archived != ArchivedState.None == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                if (isKeylolOperator)
                {
                    activity.Archived = requestDto.Value ? ArchivedState.Operator : ArchivedState.None;
                }
                else
                {
                    if (activity.Archived == ArchivedState.Operator)
                        return Unauthorized();
                    activity.Archived = requestDto.Value ? ArchivedState.User : ArchivedState.None;
                }
            }
            else
            {
                if ((bool) propertyInfo.GetValue(activity) == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                propertyInfo.SetValue(activity, requestDto.Value);
            }
            await _dbContext.SaveChangesAsync();
            if (isKeylolOperator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = new Message
                {
                    OperatorId = operatorId,
                    ReceiverId = activity.AuthorId,
                    ActivityId = activity.Id
                };
                string steamNotityText = null;
                var activitySummary = PostOfficeMessageList.CollapseActivityContent(activity, 30);
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Archived:
                            missive.Type = MessageType.ActivityArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"动态「{activitySummary}」已被封存，封存后该动态的内容和所有评论会被隐藏，同时这则动态不会再显示于任何轨道上。";
                            break;

                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Rejected:
                            missive.Type = MessageType.ActivityRejection;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"动态「{activitySummary}」已被退稿，不会再出现于其他用户或据点的轨道上，这则动态后续的投稿也将被自动回绝。";
                            break;

                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Warned:
                            missive.Type = MessageType.ActivityWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"动态「{activitySummary}」已被警告，若在 30 天之内收到两次警告，你的账户将被自动停权 14 天。";
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Archived:
                            missive.Type = MessageType.ActivityArchiveCancel;
                            steamNotityText = $"动态「{activitySummary}」的封存已被撤销，该动态的内容和所有评论已重新公开，轨道将不再隐藏这则动态。";
                            break;

                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Rejected:
                            missive.Type = MessageType.ActivityRejectionCancel;
                            steamNotityText = $"动态「{activitySummary}」的退稿限制已被撤销，其他用户首页的轨道将不再隐藏这则动态，后续的投稿也不再会被其他据点回绝。";
                            break;

                        case ActivityUpdateOneModerationRequestDto.ActivityProperty.Warned:
                            missive.Type = MessageType.ActivityWarningCancel;
                            steamNotityText = $"动态「{activitySummary}」的警告已被撤销，之前的警告将不再纳入停权计数器的考量中，除非你的账户已经因收到警告而被自动停权。";
                            break;
                    }
                }
                await _cachedData.Messages.AddAsync(missive);

                // Steam 通知

                if (!string.IsNullOrWhiteSpace(steamNotityText))
                    await _userManager.SendSteamChatMessageAsync(activity.Author, steamNotityText);
            }
            return Ok();
        }
    }

    /// <summary>
    ///     封存、退稿、萃选、警告请求 DTO
    /// </summary>
    public class ActivityUpdateOneModerationRequestDto
    {
        /// <summary>
        ///     动态属性
        /// </summary>
        public enum ActivityProperty
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
            ///     警告状态
            /// </summary>
            Warned
        }

        /// <summary>
        ///     要操作的动态属性
        /// </summary>
        public ActivityProperty Property { get; set; }

        /// <summary>
        ///     动态属性的新值
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