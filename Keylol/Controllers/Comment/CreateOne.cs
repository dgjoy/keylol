using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        ///     创建一条评论
        /// </summary>
        /// <param name="requestDto">评论相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(CommentDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "指定文章被封存，当前用户无权创建新评论")]
        public async Task<IHttpActionResult> CreateOne([NotNull] CommentCreateOneRequestDto requestDto)
        {
            var article = await _dbContext.Articles.FindAsync(requestDto.ArticleId);
            if (article == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.ArticleId), Errors.NonExistent);

            var userId = User.Identity.GetUserId();
            if (article.Archived != ArchivedState.None &&
                userId != article.PrincipalId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            var replyToComments = await _dbContext.Comments.Include(c => c.Commentator.SteamBot)
                .Where(
                    c =>
                        c.ArticleId == article.Id &&
                        requestDto.ReplyToCommentsSn.Contains(c.SequenceNumberForArticle))
                .ToListAsync();

            var comment = _dbContext.Comments.Create();
            _dbContext.Comments.Add(comment);
            comment.ArticleId = article.Id;
            comment.CommentatorId = userId;
            comment.Content = requestDto.Content;
            comment.SequenceNumberForArticle = _dbContext.Comments.Where(c => c.ArticleId == article.Id)
                .Select(c => c.SequenceNumberForArticle)
                .DefaultIfEmpty(0)
                .Max() + 1;
            _dbContext.SaveChanges();
            var commentReplies = replyToComments.Select(c => new CommentReply
            {
                Comment = c,
                ReplyId = comment.Id
            }).ToList();
            _dbContext.CommentReplies.AddRange(commentReplies);
            await _dbContext.SaveChangesAsync();

            var articleAuthor = await _dbContext.Users.Include(u => u.SteamBot)
                .SingleAsync(u => u.Id == article.PrincipalId);
            var messageNotifiedArticleAuthor = false;
            var steamNotifiedArticleAuthor = false;
            const int truncateContentTo = 512;
            var truncatedContent = truncateContentTo < comment.Content.Length
                ? $"{comment.Content.Substring(0, truncateContentTo)} …"
                : comment.Content;
            foreach (var replyToUser in
                commentReplies.Where(
                    cr => !(cr.Comment.CommentatorId == comment.CommentatorId || cr.Comment.IgnoreNewComments))
                    .Select(cr => cr.Comment.Commentator)
                    .Distinct())
            {
                if (replyToUser.Id == articleAuthor.Id)
                {
                    messageNotifiedArticleAuthor = true;
                    steamNotifiedArticleAuthor = replyToUser.SteamNotifyOnCommentReplied;
                }

                // 邮政中心
                var message = _dbContext.Messages.Create();
                message.Type = MessageType.CommentReply;
                message.OperatorId = comment.CommentatorId;
                message.ReceiverId = replyToUser.Id;
                message.CommentId = comment.Id;
                _dbContext.Messages.Add(message);

                if (!replyToUser.SteamNotifyOnCommentReplied)
                    continue;

                // Steam 通知
                await _userManager.SendSteamChatMessageAsync(replyToUser,
                    $"@{comment.Commentator.UserName} 回复了你在 《{article.Title}》 下的评论：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
            }
            if (!(comment.CommentatorId == article.PrincipalId || article.IgnoreNewComments))
            {
                if (!messageNotifiedArticleAuthor)
                {
                    // 邮政中心
                    var message = _dbContext.Messages.Create();
                    message.Type = MessageType.ArticleComment;
                    message.OperatorId = comment.CommentatorId;
                    message.ReceiverId = articleAuthor.Id;
                    message.CommentId = comment.Id;
                    _dbContext.Messages.Add(message);
                }

                if (!steamNotifiedArticleAuthor && articleAuthor.SteamNotifyOnArticleReplied)
                {
                    // Steam 通知
                    await _userManager.SendSteamChatMessageAsync(articleAuthor,
                        $"@{comment.Commentator.UserName} 评论了你的文章 《{article.Title}》：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                }
            }
            await _dbContext.SaveChangesAsync();

            return Created($"comment/{comment.Id}", new CommentDto(comment, false));
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class CommentCreateOneRequestDto
        {
            /// <summary>
            ///     评论内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            ///     评论的文章 Id
            /// </summary>
            [Required]
            public string ArticleId { get; set; }

            /// <summary>
            ///     回复的楼层号列表
            /// </summary>
            [Required]
            public List<int> ReplyToCommentsSn { get; set; }
        }
    }
}