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
        ///     创建一条文章评论
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "评论楼层号")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ArticleCommentCreateOneRequestDto requestDto)
        {
            var article = await _dbContext.Articles.Include(a => a.Author)
                .Where(a => a.Id == requestDto.ArticleId)
                .SingleOrDefaultAsync();
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
                UnstyledContent = PlainTextFormatter.FlattenHtml(requestDto.Content, false),
                SidForArticle = await _dbContext.ArticleComments.Where(c => c.ArticleId == article.Id)
                    .Select(c => c.SidForArticle)
                    .DefaultIfEmpty(0)
                    .MaxAsync() + 1
            };

            if (requestDto.ReplyToComment != null)
            {
                var replyToComment = await _dbContext.ArticleComments
                    .Include(c => c.Commentator)
                    .Where(c => c.ArticleId == article.Id && c.SidForArticle == requestDto.ReplyToComment)
                    .SingleOrDefaultAsync();
                if (replyToComment != null)
                    comment.ReplyToComment = replyToComment;
            }

            _dbContext.ArticleComments.Add(comment);
            await _dbContext.SaveChangesAsync();
            await _cachedData.ArticleComments.IncreaseArticleCommentCountAsync(article.Id, 1);

            var messageNotifiedArticleAuthor = false;
            var steamNotifiedArticleAuthor = false;
            var unstyledContentWithNewLine = PlainTextFormatter.FlattenHtml(comment.Content, true);
            unstyledContentWithNewLine = unstyledContentWithNewLine.Length > 512
                ? $"{unstyledContentWithNewLine.Substring(0, 512)} …"
                : unstyledContentWithNewLine;
            if (comment.ReplyToComment != null && comment.ReplyToComment.CommentatorId != comment.CommentatorId &&
                !comment.ReplyToComment.DismissReplyMessage)
            {
                if (comment.ReplyToComment.Commentator.NotifyOnCommentReplied)
                {
                    messageNotifiedArticleAuthor = comment.ReplyToComment.CommentatorId == article.AuthorId;
                    _dbContext.Messages.Add(new Message
                    {
                        Type = MessageType.ArticleCommentReply,
                        OperatorId = comment.CommentatorId,
                        ReceiverId = comment.ReplyToComment.CommentatorId,
                        ArticleCommentId = comment.Id
                    });
                }

                if (comment.ReplyToComment.Commentator.SteamNotifyOnCommentReplied)
                {
                    steamNotifiedArticleAuthor = comment.ReplyToComment.CommentatorId == article.AuthorId;
                    await _userManager.SendSteamChatMessageAsync(comment.ReplyToComment.Commentator,
                        $"@{comment.Commentator.UserName} 回复了你在 《{article.Title}》 下的评论：\n\n{unstyledContentWithNewLine}\n\nhttps://www.keylol.com/article/{article.Author.IdCode}/{article.SidForAuthor}#{comment.SidForArticle}");
                }
            }

            if (comment.CommentatorId != article.AuthorId && !article.DismissCommentMessage)
            {
                if (!messageNotifiedArticleAuthor && article.Author.NotifyOnArticleReplied)
                {
                    _dbContext.Messages.Add(new Message
                    {
                        Type = MessageType.ArticleComment,
                        OperatorId = comment.CommentatorId,
                        ReceiverId = article.AuthorId,
                        ArticleCommentId = comment.Id
                    });
                }

                if (!steamNotifiedArticleAuthor && article.Author.SteamNotifyOnArticleReplied)
                {
                    await _userManager.SendSteamChatMessageAsync(article.Author,
                        $"@{comment.Commentator.UserName} 评论了你的文章 《{article.Title}》：\n\n{unstyledContentWithNewLine}\n\nhttps://www.keylol.com/article/{article.Author.IdCode}/{article.SidForAuthor}#{comment.SidForArticle}");
                }
            }
            await _dbContext.SaveChangesAsync();

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