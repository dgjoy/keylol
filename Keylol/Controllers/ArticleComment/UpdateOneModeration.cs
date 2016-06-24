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
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.ArticleComment
{
    public partial class ArticleCommentController
    {
        /// <summary>
        ///     更新文章评论的封存或警告状态
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}/moderation")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权操作这个评论")]
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            [NotNull] ArticleCommentUpdateOneModerationRequestDto requestDto)
        {
            var comment = await _dbContext.ArticleComments.Include(c => c.Commentator).Include(c => c.Article)
                .Where(a => a.Id == id).SingleOrDefaultAsync();
            if (comment == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var isKeylolOperator = User.IsInRole(KeylolRoles.Operator);

            if (!isKeylolOperator)
            {
                switch (requestDto.Property)
                {
                    case ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                        if (comment.CommentatorId != operatorId)
                            return Unauthorized();
                        break;
                    default:
                        return Unauthorized();
                }
            }

            if (
                !Enum.IsDefined(typeof(ArticleCommentUpdateOneModerationRequestDto.CommentProperty), requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof(Models.ArticleComment).GetProperty(requestDto.Property.ToString());
            if (requestDto.Property == ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Archived)
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
            if (isKeylolOperator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = new Message
                {
                    OperatorId = operatorId,
                    ReceiverId = comment.CommentatorId,
                    ArticleCommentId = comment.Id
                };
                string steamNotityText = null;
                var commentSummary = comment.UnstyledContent.Length > 30
                    ? $"{comment.UnstyledContent.Substring(0, 30)} …"
                    : comment.UnstyledContent;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.ArticleCommentArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText =
                                $"文章《{comment.Article.Title}》中的评论「{commentSummary}」已被封存，封存后此则评论的内容和作者信息会被隐藏。";
                            break;

                        case ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.ArticleCommentWarning;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText =
                                $"文章《{comment.Article.Title}》中的评论「{commentSummary}」已被警告，若在 30 天之内收到两次警告，你的账户将被自动停权 14 天。";
                            break;
                    }
                }
                else
                {
                    switch (requestDto.Property)
                    {
                        case ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Archived:
                            missive.Type = MessageType.ArticleCommentArchiveCancel;
                            steamNotityText =
                                $"文章《{comment.Article.Title}》下评论「{commentSummary}」的封存已被撤销，此则评论的内容和作者信息已重新公开。";
                            break;

                        case ArticleCommentUpdateOneModerationRequestDto.CommentProperty.Warned:
                            missive.Type = MessageType.ArticleCommentWarningCancel;
                            steamNotityText =
                                $"文章《{comment.Article.Title}》下评论「{commentSummary}」收到的警告已被撤销，之前的警告将不再纳入停权计数器的考量中，除非你的账户已经因收到警告而被自动停权。";
                            break;
                    }
                }
                _dbContext.Messages.Add(missive);

                // Steam 通知

                if (!string.IsNullOrWhiteSpace(steamNotityText))
                    await _userManager.SendSteamChatMessageAsync(comment.Commentator, steamNotityText);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    /// <summary>
    ///     封存、警告请求 DTO
    /// </summary>
    public class ArticleCommentUpdateOneModerationRequestDto
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