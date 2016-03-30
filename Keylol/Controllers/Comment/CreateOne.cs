using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Services;
using Keylol.Services.Contracts;
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
        /// <param name="vm">评论相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (CommentDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "指定文章被封存，当前用户无权创建新评论")]
        public async Task<IHttpActionResult> CreateOne(CommentVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await DbContext.Articles.FindAsync(vm.ArticleId);
            if (article == null)
            {
                ModelState.AddModelError("vm.ArticleId", "Article doesn't exsit.");
                return BadRequest(ModelState);
            }

            var userId = User.Identity.GetUserId();
            var staffClaim = await UserManager.GetStaffClaimAsync(userId);
            if (article.Archived != ArchivedState.None &&
                userId != article.PrincipalId && staffClaim != StaffClaim.Operator)
                return Unauthorized();

            var replyToComments = await DbContext.Comments.Include(c => c.Commentator.SteamBot)
                .Where(c => c.ArticleId == article.Id && vm.ReplyToCommentsSN.Contains(c.SequenceNumberForArticle))
                .ToListAsync();

            var comment = DbContext.Comments.Create();
            DbContext.Comments.Add(comment);
            comment.ArticleId = article.Id;
            comment.CommentatorId = userId;
            comment.Content = vm.Content;
            comment.SequenceNumberForArticle = DbContext.Comments.Where(c => c.ArticleId == article.Id)
                .Select(c => c.SequenceNumberForArticle)
                .DefaultIfEmpty(0)
                .Max() + 1;
            DbContext.SaveChanges();
            var commentReplies = replyToComments.Select(c => new CommentReply
            {
                Comment = c,
                ReplyId = comment.Id
            }).ToList();
            DbContext.CommentReplies.AddRange(commentReplies);
            await DbContext.SaveChangesAsync();

            var articleAuthor = await DbContext.Users.Include(u => u.SteamBot)
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
                var message = DbContext.Messages.Create();
                message.Type = MessageType.CommentReply;
                message.OperatorId = comment.CommentatorId;
                message.ReceiverId = replyToUser.Id;
                message.CommentId = comment.Id;
                DbContext.Messages.Add(message);

                if (!replyToUser.SteamNotifyOnCommentReplied)
                    continue;

                // Steam 通知
                ISteamBotCoodinatorCallback callback;
                if (replyToUser.SteamBot.SessionId != null &&
                    SteamBotCoodinator.Clients.TryGetValue(replyToUser.SteamBot.SessionId, out callback))
                {
                    callback.SendMessage(replyToUser.SteamBotId, replyToUser.SteamId,
                        $"@{comment.Commentator.UserName} 回复了你在 《{article.Title}》 下的评论：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                }
            }
            if (!(comment.CommentatorId == article.PrincipalId || article.IgnoreNewComments))
            {
                if (!messageNotifiedArticleAuthor)
                {
                    // 邮政中心
                    var message = DbContext.Messages.Create();
                    message.Type = MessageType.ArticleComment;
                    message.OperatorId = comment.CommentatorId;
                    message.ReceiverId = articleAuthor.Id;
                    message.CommentId = comment.Id;
                    DbContext.Messages.Add(message);
                }

                if (!steamNotifiedArticleAuthor && articleAuthor.SteamNotifyOnArticleReplied)
                {
                    // Steam 通知
                    ISteamBotCoodinatorCallback callback;
                    if (articleAuthor.SteamBot.SessionId != null &&
                        SteamBotCoodinator.Clients.TryGetValue(articleAuthor.SteamBot.SessionId, out callback))
                    {
                        callback.SendMessage(articleAuthor.SteamBotId, articleAuthor.SteamId,
                            $"@{comment.Commentator.UserName} 评论了你的文章 《{article.Title}》：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                    }
                }
            }
            await DbContext.SaveChangesAsync();

            return Created($"comment/{comment.Id}", new CommentDTO(comment, false));
        }
    }
}