using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
        /// 创建一条动态评论
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "动态楼层号")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ActivityCommentCreateOneRequestDto requestDto)
        {
            var activity = await _dbContext.Activities.Include(a => a.Author)
                .Where(a => a.Id == requestDto.ActivityId)
                .SingleOrDefaultAsync();
            if (activity == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.ActivityId), Errors.NonExistent);

            var userId = User.Identity.GetUserId();
            if (activity.Archived != ArchivedState.None &&
                userId != activity.AuthorId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            var comment = new Models.ActivityComment
            {
                ActivityId = activity.Id,
                CommentatorId = userId,
                Content = requestDto.Content,
                SidForActivity = await _dbContext.ActivityComments.Where(c => c.ActivityId == activity.Id)
                    .Select(c => c.SidForActivity)
                    .DefaultIfEmpty(0)
                    .MaxAsync() + 1
            };

            _dbContext.ActivityComments.Add(comment);
            await _dbContext.SaveChangesAsync();
            await _cachedData.ActivityComments.IncreaseActivityCommentCountAsync(activity.Id, 1);
            
            var matches = Regex.Matches(comment.Content, "^(?:#(\\d+)[ \\t]*)+(?:$|[ \\t]+)", RegexOptions.Multiline);
            if (matches.Count <= 0) return Ok(comment.SidForActivity);

            var sidForActivities = (from Match match in matches
                from Capture capture in match.Groups[1].Captures
                select int.Parse(capture.Value)).ToList();

            var messageNotifiedArticleAuthor = false;
            var steamNotifiedArticleAuthor = false;
            var truncatedContent = comment.Content.Length > 512
                ? $"{comment.Content.Substring(0, 512)} …"
                : comment.Content;
            var activityContent = PostOfficeMessageList.CollapseActivityContent(activity);

            var replyToComments = await _dbContext.ActivityComments
                .Include(c => c.Commentator)
                .Where(c => c.ActivityId == activity.Id && sidForActivities.Contains(c.SidForActivity))
                .ToListAsync();
            foreach (var replyToComment in replyToComments)
            {
                _dbContext.Replies.Add(new Reply
                {
                    EntryType = ReplyEntryType.ActivityComment,
                    EntryId = replyToComment.Id,
                    ReplyId = comment.Id
                });
            }

            foreach (var replyToUser in replyToComments
                .Where(c => c.CommentatorId != comment.CommentatorId && !c.DismissReplyMessage)
                .Select(c => c.Commentator).Distinct())
            {
                if (replyToUser.NotifyOnCommentReplied)
                {
                    messageNotifiedArticleAuthor = replyToUser.Id == activity.AuthorId;
                    _dbContext.Messages.Add(new Message
                    {
                        Type = MessageType.ActivityCommentReply,
                        OperatorId = comment.CommentatorId,
                        ReceiverId = replyToUser.Id,
                        ActivityCommentId = comment.Id
                    });
                }

                if (replyToUser.SteamNotifyOnCommentReplied)
                {
                    steamNotifiedArticleAuthor = replyToUser.Id == activity.AuthorId;
                    await _userManager.SendSteamChatMessageAsync(replyToUser,
                        $"{comment.Commentator.UserName} 回复了你在「{activityContent}」下的评论：\n\n{truncatedContent}\n\nhttps://www.keylol.com/activity/{activity.Author.IdCode}/{activity.SidForAuthor}#{comment.SidForActivity}");
                }
            }

            if (comment.CommentatorId != activity.AuthorId && !activity.DismissCommentMessage)
            {
                if (!messageNotifiedArticleAuthor && activity.Author.NotifyOnActivityReplied)
                {
                    _dbContext.Messages.Add(new Message
                    {
                        Type = MessageType.ActivityComment,
                        OperatorId = comment.CommentatorId,
                        ReceiverId = activity.AuthorId,
                        ActivityCommentId = comment.Id
                    });
                }

                if (!steamNotifiedArticleAuthor && activity.Author.SteamNotifyOnActivityReplied)
                {
                    await _userManager.SendSteamChatMessageAsync(activity.Author,
                        $"{comment.Commentator.UserName} 评论了你的动态「{activityContent}」：\n\n{truncatedContent}\n\nhttps://www.keylol.com/activity/{activity.Author.IdCode}/{activity.SidForAuthor}#{comment.SidForActivity}");
                }
            }
            await _dbContext.SaveChangesAsync();

            return Ok(comment.SidForActivity);
        }

        /// <summary>
        /// 请求 DTO
        /// </summary>
        public class ActivityCommentCreateOneRequestDto
        {
            /// <summary>
            ///     内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            ///     动态 ID
            /// </summary>
            [Required]
            public string ActivityId { get; set; }
        }
    }
}