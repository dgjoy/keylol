using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        /// 更新文章的封存、退稿、萃选或警告状态
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
                        break;
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
                missive.ReceiverId = article.PrincipalId;
                missive.ArticleId = article.Id;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                            missive.Type = MessageType.ArticleArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.Rejection;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                            missive.Type = MessageType.Spotlight;
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Warned:
                            missive.Type = MessageType.ArticleWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                            missive.Type = MessageType.ArticleArchiveCancel;
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.RejectionCancel;
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                            missive.Type = MessageType.SpotlightCancel;
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Warned:
                            missive.Type = MessageType.ArticleWarningCancel;
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
    /// 封存、退稿、萃选、警告请求 DTO
    /// </summary>
    public class ArticleUpdateOneModerationRequestDto
    {
        /// <summary>
        /// 文章属性
        /// </summary>
        public enum ArticleProperty
        {
            /// <summary>
            /// 封存状态
            /// </summary>
            Archived,

            /// <summary>
            /// 退稿状态
            /// </summary>
            Rejected,

            /// <summary>
            /// 萃选状态
            /// </summary>
            Spotlight,

            /// <summary>
            /// 警告状态
            /// </summary>
            Warned
        }

        /// <summary>
        /// 要操作的文章属性
        /// </summary>
        public ArticleProperty Property { get; set; }

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