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

namespace Keylol.Controllers.ActivityComment
{
    public partial class ActivityCommentController
    {
        /// <summary>
        ///     更新动态评论的封存或警告状态
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}/moderation")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权操作这个评论")]
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            [NotNull] ActivityCommentUpdateOneModerationRequestDto requestDto)
        {
            var comment = await _dbContext.ActivityComments.Include(c => c.Commentator).Include(c => c.Activity)
                .Where(a => a.Id == id).SingleOrDefaultAsync();
            if (comment == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var isKeylolOperator = User.IsInRole(KeylolRoles.Operator);

            if (!isKeylolOperator)
            {
                switch (requestDto.Property)
                {
                    case ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                        if (comment.CommentatorId != operatorId)
                            return Unauthorized();
                        break;
                    default:
                        return Unauthorized();
                }
            }

            if (
                !Enum.IsDefined(typeof(ActivityCommentUpdateOneModerationRequestDto.CommentProperty),
                    requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof(Models.ActivityComment).GetProperty(requestDto.Property.ToString());
            if (requestDto.Property == ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Archived)
            {
                if (comment.Archived != ArchivedState.None == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                if (isKeylolOperator)
                {
                    comment.Archived = requestDto.Value ? ArchivedState.Operator : ArchivedState.None;
                }
                else
                {
                    if (comment.Archived == ArchivedState.Operator)
                        return Unauthorized();
                    comment.Archived = requestDto.Value ? ArchivedState.User : ArchivedState.None;
                }
            }
            else
            {
                if ((bool) propertyInfo.GetValue(comment) == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                propertyInfo.SetValue(comment, requestDto.Value);
            }
            await _dbContext.SaveChangesAsync();
            if (isKeylolOperator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = new Message
                {
                    OperatorId = operatorId,
                    ReceiverId = comment.CommentatorId,
                    ActivityCommentId = comment.Id
                };
                string steamNotityText = null;
                var activitySummary = PostOfficeMessageList.CollapseActivityContent(comment.Activity, 30);
                var commentSummary = comment.Content.Length > 30
                    ? $"{comment.Content.Substring(0, 30)} …"
                    : comment.Content;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.ActivityCommentArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText =
                                $"动态「{activitySummary}」中的评论「{commentSummary}」已被封存，封存后此则评论的内容和作者信息会被隐藏。";
                            break;

                        case ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.ActivityCommentWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText =
                                $"动态「{activitySummary}」中的评论「{commentSummary}」已被警告，若在 30 天之内收到两次警告，你的账户将被自动停权 14 天。";
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.ActivityCommentArchiveCancel;
                            steamNotityText =
                                $"动态「{activitySummary}」下评论「{commentSummary}」的封存已被撤销，此则评论的内容和作者信息已重新公开。";
                            break;

                        case ActivityCommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.ActivityCommentWarningCancel;
                            steamNotityText =
                                $"动态「{activitySummary}」下评论「{commentSummary}」收到的警告已被撤销，之前的警告将不再纳入停权计数器的考量中，除非你的账户已经因收到警告而被自动停权。";
                            break;
                    }
                }
                await _cachedData.Messages.AddAsync(missive);

                // Steam 通知

                if (!string.IsNullOrWhiteSpace(steamNotityText))
                    await _userManager.SendSteamChatMessageAsync(comment.Commentator, steamNotityText);
            }
            return Ok();
        }
    }

    /// <summary>
    ///     封存、警告请求 DTO
    /// </summary>
    public class ActivityCommentUpdateOneModerationRequestDto
    {
        /// <summary>
        ///     评论属性
        /// </summary>
        public enum CommentProperty
        {
            /// <summary>
            ///     封存状态
            /// </summary>
            Archived,

            /// <summary>
            ///     警告状态
            /// </summary>
            Warned
        }

        /// <summary>
        ///     要操作的评论属性
        /// </summary>
        public CommentProperty Property { get; set; }

        /// <summary>
        ///     评论属性的新值
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