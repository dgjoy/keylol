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
        public async Task<IHttpActionResult> UpdoteOneModeration(string id,
            [NotNull] ArticleUpdateOneModerationRequestDto requestDto)
        {
            var article = await _dbContext.Articles.Include(a => a.Author).Where(a => a.Id == id).SingleOrDefaultAsync();
            if (article == null)
                return NotFound();

            var operatorId = User.Identity.GetUserId();
            var isKeylolOperator = User.IsInRole(KeylolRoles.Operator);

            if (!isKeylolOperator)
            {
                switch (requestDto.Property)
                {
                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                        if (article.AuthorId != operatorId)
                            return Unauthorized();
                        break;

                    case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                        if (article.AuthorId != operatorId || requestDto.Value)
                            return Unauthorized();
                        break;

                    default:
                        return Unauthorized();
                }
            }

            if (!Enum.IsDefined(typeof(ArticleUpdateOneModerationRequestDto.ArticleProperty), requestDto.Property))
                throw new ArgumentOutOfRangeException(nameof(requestDto.Property));
            var propertyInfo = typeof(Models.Article).GetProperty(requestDto.Property.ToString());
            if (requestDto.Property == ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived)
            {
                if (article.Archived != ArchivedState.None == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                if (isKeylolOperator)
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
            else
            {
                if ((bool) propertyInfo.GetValue(article) == requestDto.Value)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Value), Errors.Duplicate);

                propertyInfo.SetValue(article, requestDto.Value);
            }
            if (isKeylolOperator && (requestDto.NotifyAuthor ?? false))
            {
                var missive = new Message
                {
                    OperatorId = operatorId,
                    ReceiverId = article.AuthorId,
                    ArticleId = article.Id
                };
                string steamNotityText = null;
                if (requestDto.Value)
                {
                    switch (requestDto.Property)
                    {
                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Archived:
                            missive.Type = MessageType.ArticleArchive;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"文章《{article.Title}》已被封存，封存后该文章的内容和所有评论会被隐藏，同时这篇文章不会再显示于任何轨道上。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.ArticleRejection;
                            if (requestDto.Reasons != null)
                                missive.Reasons = string.Join(",", requestDto.Reasons);
                            steamNotityText = $"文章《{article.Title}》已被退稿，不会再出现于其他用户或据点的轨道上，这篇文章后续的投稿也将被自动回绝。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Spotlight:
                            missive.Type = MessageType.Spotlight;
                            steamNotityText =
                                $"感谢你对其乐社区质量的认可与贡献！你的文章《{article.Title}》已被推荐为萃选文章。";
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
                            steamNotityText = $"文章《{article.Title}》的封存已被撤销，该文章的内容和所有评论已重新公开，轨道将不再隐藏这篇文章。";
                            break;

                        case ArticleUpdateOneModerationRequestDto.ArticleProperty.Rejected:
                            missive.Type = MessageType.ArticleRejectionCancel;
                            steamNotityText = $"文章《{article.Title}》的退稿限制已被撤销，其他用户首页的轨道将不再隐藏这篇文章，后续的投稿也不再会被其他据点回绝。";
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
                _dbContext.Messages.Add(missive);

                // Steam 通知

                if (!string.IsNullOrWhiteSpace(steamNotityText))
                    await _userManager.SendSteamChatMessageAsync(article.Author, steamNotityText);
            }
            await _dbContext.SaveChangesAsync();
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