using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        /// 更新评论的封存或警告状态
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}/moderation")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权操作这个评论")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            CommentUpdateOneModerationRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("requestDto", "Invalid request DTO.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await DbContext.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var operatorStaffClaim = await UserManager.GetStaffClaimAsync(operatorId);

            if (operatorStaffClaim != StaffClaim.Operator)
            {
                switch (requestDto.Property)
                {
                    case CommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                        if (comment.CommentatorId != operatorId)
                            return Unauthorized();
                        break;
                    default:
                        return Unauthorized();
                }
            }

            if (!Enum.IsDefined(typeof (CommentUpdateOneModerationRequestDto.CommentProperty), requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof (Models.Comment).GetProperty(requestDto.Property.ToString());
            if ((bool) propertyInfo.GetValue(comment) == requestDto.Value)
            {
                ModelState.AddModelError("requestDto.Value", "评论已经处于目标状态");
                return BadRequest(ModelState);
            }
            propertyInfo.SetValue(comment, requestDto.Value);
            if (operatorStaffClaim == StaffClaim.Operator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = DbContext.Messages.Create();
                missive.OperatorId = operatorId;
                missive.ReceiverId = comment.CommentatorId;
                missive.CommentId = comment.Id;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case CommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.CommentArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            break;

                        case CommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.CommentWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case CommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.CommentArchiveCancel;
                            break;

                        case CommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.CommentWarningCancel;
                            break;
                    }
                }
                await DbContext.GiveNextSequenceNumberAsync(missive);
                DbContext.Messages.Add(missive);
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }

    /// <summary>
    /// 封存、警告请求 DTO
    /// </summary>
    public class CommentUpdateOneModerationRequestDto
    {
        /// <summary>
        /// 评论属性
        /// </summary>
        public enum CommentProperty
        {
            /// <summary>
            /// 封存状态
            /// </summary>
            Archived,

            /// <summary>
            /// 警告状态
            /// </summary>
            Warned
        }

        /// <summary>
        /// 要操作的评论属性
        /// </summary>
        public CommentProperty Property { get; set; }

        /// <summary>
        /// 文章属性的新值
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// 操作理由
        /// </summary>
        public List<int> Reasons { get; set; }

        /// <summary>
        /// 是否通知作者
        /// </summary>
        public bool? NotifyAuthor { get; set; }
    }
}