using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            if (comment.CommentatorId == article.PrincipalId)
            {
                comment.IgnoredByArticleAuthor = true;
                comment.ReadByArticleAuthor = true;
            }
            else if (article.IgnoreNewComments)
            {
                comment.IgnoredByArticleAuthor = true;
            }
            comment.SequenceNumberForArticle = DbContext.Comments.Where(c => c.ArticleId == article.Id)
                .Select(c => c.SequenceNumberForArticle)
                .DefaultIfEmpty(0)
                .Max() + 1;
            DbContext.SaveChanges();
            var commentReplies = replyToComments.Select(c => new CommentReply
            {
                Comment = c,
                IgnoredByCommentAuthor = c.CommentatorId == comment.CommentatorId || c.IgnoreNewComments,
                ReadByCommentAuthor = c.CommentatorId == comment.CommentatorId,
                ReplyId = comment.Id
            }).ToList();
            DbContext.CommentReplies.AddRange(commentReplies);
            await DbContext.SaveChangesAsync();

            var articleAuthor = await DbContext.Users.Include(u => u.SteamBot)
                .SingleAsync(u => u.Id == article.PrincipalId);
            var notifiedArticleAuthor = false;
            const int truncateContentTo = 512;
            var truncatedContent = truncateContentTo < comment.Content.Length
                ? $"{comment.Content.Substring(0, truncateContentTo)} …"
                : comment.Content;
            foreach (var replyToUser in
                commentReplies.Where(cr => !cr.IgnoredByCommentAuthor)
                    .Select(cr => cr.Comment.Commentator)
                    .Distinct())
            {
                if (!replyToUser.SteamNotifyOnCommentReplied)
                    continue;

                if (replyToUser.Id == articleAuthor.Id)
                    notifiedArticleAuthor = true;
                ISteamBotCoodinatorCallback callback;
                if (replyToUser.SteamBot.SessionId != null &&
                    SteamBotCoodinator.Clients.TryGetValue(replyToUser.SteamBot.SessionId,
                    out callback))
                {
                    callback.SendMessage(replyToUser.SteamBotId, replyToUser.SteamId,
                        $"@{comment.Commentator.UserName} 回复了你在 《{article.Title}》 下的评论：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                }
            }
            if (!notifiedArticleAuthor && !comment.IgnoredByArticleAuthor && articleAuthor.SteamNotifyOnArticleReplied)
            {
                ISteamBotCoodinatorCallback callback;
                if (articleAuthor.SteamBot.SessionId != null && 
                    SteamBotCoodinator.Clients.TryGetValue(articleAuthor.SteamBot.SessionId, out callback))
                {
                    callback.SendMessage(articleAuthor.SteamBotId, articleAuthor.SteamId,
                        $"@{comment.Commentator.UserName} 评论了你的文章 《{article.Title}》：\n{truncatedContent}\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                }
            }

            return Created($"comment/{comment.Id}", new CommentDTO(comment, false));
        }
    }
}