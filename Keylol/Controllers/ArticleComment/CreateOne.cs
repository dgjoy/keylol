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
        ///     创建一条评论
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "评论楼层号")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ArticleCommentCreateOneRequestDto requestDto)
        {
            var article = await _dbContext.Articles.FindAsync(requestDto.ArticleId);
            if (article == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.ArticleId), Errors.NonExistent);

            var userId = User.Identity.GetUserId();
            if (article.Archived != ArchivedState.None &&
                userId != article.AuthorId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            var comment = new Models.ArticleComment
            {
                ArticleId = article.Id,
                CommentatorId = userId,
                Content = requestDto.Content,
                SidForArticle = await _dbContext.ArticleComments.Where(c => c.ArticleId == article.Id)
                    .Select(c => c.SidForArticle)
                    .DefaultIfEmpty(0)
                    .MaxAsync() + 1
            };

            if (requestDto.ReplyToComment != null)
            {
                var replyToComment = await _dbContext.ArticleComments
                    .Where(c => c.ArticleId == article.Id && c.SidForArticle == requestDto.ReplyToComment)
                    .SingleOrDefaultAsync();
                if (replyToComment != null)
                    comment.ReplyToComment = replyToComment;
            }

            _dbContext.ArticleComments.Add(comment);
            _dbContext.SaveChanges();

            // TODO: 通知推送
//            var articleAuthor = await _dbContext.Users.Include(u => u.SteamBot)
//                .SingleAsync(u => u.Id == article.AuthorId);
//            var messageNotifiedArticleAuthor = false;
//            var steamNotifiedArticleAuthor = false;
//            const int truncateContentTo = 512;
//            var truncatedContent = truncateContentTo < comment.Content.Length
//                ? $"{comment.Content.Substring(0, truncateContentTo)} …"
//                : comment.Content;
//            foreach (var replyToUser in
//                commentReplies.Where(
//                    cr => !(cr.Comment.CommentatorId == comment.CommentatorId || cr.Comment.IgnoreNewComments))
//                    .Select(cr => cr.Comment.Commentator)
//                    .Distinct())
//            {
//                if (replyToUser.Id == articleAuthor.Id)
//                {
//                    messageNotifiedArticleAuthor = true;
//                    steamNotifiedArticleAuthor = replyToUser.SteamNotifyOnCommentReplied;
//                }
//
//                // 邮政中心
//                var message = _dbContext.Messages.Create();
//                message.Type = MessageType.CommentReply;
//                message.OperatorId = comment.CommentatorId;
//                message.ReceiverId = replyToUser.Id;
//                message.CommentId = comment.Id;
//                _dbContext.Messages.Add(message);
//
//                if (!replyToUser.SteamNotifyOnCommentReplied)
//                    continue;
//
//                // Steam 通知
//                await _userManager.SendSteamChatMessageAsync(replyToUser,
//                    $"@{comment.Commentator.UserName} 回复了你在 《{article.Title}》 下的评论：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
//            }
//            if (!(comment.CommentatorId == article.PrincipalId || article.IgnoreNewComments))
//            {
//                if (!messageNotifiedArticleAuthor)
//                {
//                    // 邮政中心
//                    var message = _dbContext.Messages.Create();
//                    message.Type = MessageType.ArticleComment;
//                    message.OperatorId = comment.CommentatorId;
//                    message.ReceiverId = articleAuthor.Id;
//                    message.CommentId = comment.Id;
//                    _dbContext.Messages.Add(message);
//                }
//
//                if (!steamNotifiedArticleAuthor && articleAuthor.SteamNotifyOnArticleReplied)
//                {
//                    // Steam 通知
//                    await _userManager.SendSteamChatMessageAsync(articleAuthor,
//                        $"@{comment.Commentator.UserName} 评论了你的文章 《{article.Title}》：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
//                }
//            }
//            await _dbContext.SaveChangesAsync();

            return Ok(comment.SidForArticle);
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class ArticleCommentCreateOneRequestDto
        {
            /// <summary>
            ///     内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            ///     文章 ID
            /// </summary>
            [Required]
            public string ArticleId { get; set; }

            /// <summary>
            /// 回复的楼层号
            /// </summary>
            public int? ReplyToComment { get; set; }
        }
    }
}