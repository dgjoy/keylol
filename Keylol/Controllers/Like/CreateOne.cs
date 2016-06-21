using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.States.PostOffice;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     创建一个认可
        /// </summary>
        /// <param name="targetId">目标 ID</param>
        /// <param name="targetType">目标类型</param>
        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOne(string targetId, LikeTargetType targetType)
        {
            var operatorId = User.Identity.GetUserId();
            var succeed = await _cachedData.Likes.AddAsync(operatorId, targetId, targetType);
            if (succeed)
            {
                KeylolUser targetUser;
                bool notify, steamNotify;
                MessageType messageType;
                string steamNotifyText;
                var @operator = await _userManager.FindByIdAsync(operatorId);
                switch (targetType)
                {
                    case LikeTargetType.Article:
                        messageType = MessageType.ArticleLike;
                        var article = await _dbContext.Articles.Where(a => a.Id == targetId)
                            .Select(a => new
                            {
                                a.Title,
                                a.Author,
                                a.SidForAuthor
                            }).SingleAsync();
                        targetUser = article.Author;
                        notify = targetUser.NotifyOnArticleLiked;
                        steamNotify = targetUser.SteamNotifyOnArticleLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你的文章 《{article.Title}》：\nhttps://www.keylol.com/article/{targetUser.IdCode}/{article.SidForAuthor}";
                        break;

                    case LikeTargetType.ArticleComment:
                        messageType = MessageType.ArticleCommentLike;
                        var articleComment = await _dbContext.ArticleComments.Where(c => c.Id == targetId)
                            .Select(c => new
                            {
                                c.Commentator,
                                c.SidForArticle,
                                ArticleTitle = c.Article.Title,
                                ArticleSidForAuthor = c.Article.SidForAuthor,
                                ArticleAuthorIdCode = c.Article.Author.IdCode
                            }).SingleAsync();
                        targetUser = articleComment.Commentator;
                        notify = targetUser.NotifyOnCommentLiked;
                        steamNotify = targetUser.SteamNotifyOnCommentLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你在 《{articleComment.ArticleTitle}》 下的评论：\nhttps://www.keylol.com/article/{articleComment.ArticleAuthorIdCode}/{articleComment.ArticleSidForAuthor}#{articleComment.SidForArticle}";
                        break;

                    case LikeTargetType.Activity:
                        messageType = MessageType.ActivityLike;
                        var activity = await _dbContext.Activities.Include(a => a.Author)
                            .Where(a => a.Id == targetId).SingleAsync();
                        targetUser = activity.Author;
                        notify = targetUser.NotifyOnActivityLiked;
                        steamNotify = targetUser.SteamNotifyOnActivityLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你的动态 《{PostOfficeMessageList.CollapseActivityContent(activity)}》：\nhttps://www.keylol.com/activity/{targetUser.IdCode}/{activity.SidForAuthor}";
                        break;

                    case LikeTargetType.ActivityComment:
                        messageType = MessageType.ActivityCommentLike;
                        var activityComment = await _dbContext.ActivityComments.Where(c => c.Id == targetId)
                            .Select(c => new
                            {
                                c.Commentator,
                                c.SidForActivity,
                                c.Activity,
                                ActivityAuthorIdCode = c.Activity.Author.IdCode
                            }).SingleAsync();
                        targetUser = activityComment.Commentator;
                        notify = targetUser.NotifyOnCommentLiked;
                        steamNotify = targetUser.SteamNotifyOnCommentLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你在 《{PostOfficeMessageList.CollapseActivityContent(activityComment.Activity)}》 下的评论：\nhttps://www.keylol.com/activity/{activityComment.ActivityAuthorIdCode}/{activityComment.Activity.SidForAuthor}#{activityComment.SidForActivity}";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }
                if (notify)
                {
                    _dbContext.Messages.Add(new Message
                    {
                        Type = messageType,
                        OperatorId = operatorId,
                        ReceiverId = targetUser.Id,
                        Count = await _cachedData.Likes.GetUserLikeCountAsync(targetUser.Id),
                        SecondCount = await _cachedData.Likes.GetTargetLikeCountAsync(targetId, targetType)
                    });
                    await _dbContext.SaveChangesAsync();
                }
                if (steamNotify)
                {
                    await _userManager.SendSteamChatMessageAsync(targetUser, steamNotifyText);
                }
            }
            return Ok();

//            if (@operator.FreeLike <= 0 && !await _coupon.CanTriggerEventAsync(operatorId, CouponEvent.发出认可))
//                return Unauthorized();
//            var free = false;
//            switch (requestDto.TargetType)
//            {
//                case LikeTargetType.Article:
//                {
//                    var article = await _dbContext.Articles.FindAsync(requestDto.TargetId);
//                    if (article == null)
//                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.NonExistent);
//                    if (article.AuthorId == operatorId)
//                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Invalid);
//                    if (article.Archived != ArchivedState.None)
//                        return Unauthorized();
//                    if (!article.DismissLikeMessage)
//                    {
//                        var articleAuthor = await _dbContext.Users.Include(u => u.SteamBot)
//                            .SingleAsync(u => u.Id == article.PrincipalId);
//
//                        // 邮政中心
//                        var message = _dbContext.Messages.Create();
//                        message.Type = MessageType.ArticleLike;
//                        message.OperatorId = operatorId;
//                        message.ReceiverId = articleAuthor.Id;
//                        message.ArticleId = article.Id;
//                        _dbContext.Messages.Add(message);
//
//                        // Steam 通知
//
//                        if (articleAuthor.SteamNotifyOnArticleLiked)
//                            await _userManager.SendSteamChatMessageAsync(articleAuthor,
//                                $"@{@operator.UserName} 认可了你的文章 《{article.Title}》：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}");
//                    }

            // TODO: 文券变动
//                    await _coupon.Update(author, CouponEvent.获得认可, new
//                    {
//                        ArticleId = article.Id,
//                        OperatorId = operatorId
//                    });
//                    if (@operator.FreeLike > 0)
//                    {
//                        @operator.FreeLike--;
//                        free = true;
//                    }
//                    else
//                    {
//                        await _coupon.Update(@operator, CouponEvent.发出认可, new {ArticleId = article.Id});
//                    }
//                    break;
//                }
//
//                case LikeType.CommentLike:
//                {
//                    var existLike = await _dbContext.CommentLikes.FirstOrDefaultAsync(
//                        l => l.CommentId == requestDto.TargetId && l.OperatorId == operatorId);
//                    if (existLike != null)
//                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Duplicate);
//                    var comment =
//                        await
//                            _dbContext.Comments.Include(c => c.Article)
//                                .SingleOrDefaultAsync(c => c.Id == requestDto.TargetId);
//                    if (comment == null)
//                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.NonExistent);
//                    if (comment.CommentatorId == operatorId)
//                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Invalid);
//                    if (comment.Archived != ArchivedState.None || comment.Article.Archived != ArchivedState.None)
//                        return Unauthorized();
//                    var commentLike = _dbContext.CommentLikes.Create();
//                    commentLike.CommentId = requestDto.TargetId;
//                    like = commentLike;
//                    if (!comment.IgnoreNewLikes)
//                    {
//                        var commentAuthor = await _dbContext.Users.Include(u => u.SteamBot)
//                            .SingleAsync(u => u.Id == comment.CommentatorId);
//                        var articleAuthor = await _userManager.FindByIdAsync(comment.Article.PrincipalId);
//
//                        // 邮政中心
//                        var message = _dbContext.Messages.Create();
//                        message.Type = MessageType.CommentLike;
//                        message.OperatorId = operatorId;
//                        message.ReceiverId = commentAuthor.Id;
//                        message.CommentId = comment.Id;
//                        _dbContext.Messages.Add(message);
//
//                        // Steam 通知
//                        if (commentAuthor.SteamNotifyOnCommentLiked)
//                            await _userManager.SendSteamChatMessageAsync(commentAuthor,
//                                $"@{@operator.UserName} 认可了你在 《{comment.Article.Title}》 下的评论：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{comment.Article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
//                    }
//                    var commentator = await _userManager.FindByIdAsync(comment.CommentatorId);
//                    await _statistics.IncreaseUserLikeCount(comment.CommentatorId);
//                    await _coupon.Update(commentator, CouponEvent.获得认可, new
//                    {
//                        CommentId = comment.Id,
//                        OperatorId = operatorId
//                    });
//                    if (@operator.FreeLike > 0)
//                    {
//                        @operator.FreeLike--;
//                        free = true;
//                    }
//                    else
//                    {
//                        await _coupon.Update(@operator, CouponEvent.发出认可, new {CommentId = comment.Id});
//                    }
//                    break;
//                }
//
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//            like.OperatorId = operatorId;
//            _dbContext.Likes.Add(like);
//            await _dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
//            return Created($"like/{like.Id}", free ? "Free" : string.Empty);
        }
    }
}