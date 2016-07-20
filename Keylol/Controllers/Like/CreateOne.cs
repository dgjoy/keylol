using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.States.PostOffice;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

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
        [SwaggerResponse(HttpStatusCode.Unauthorized, "用户文券不足")]
        [SwaggerResponse(HttpStatusCode.OK, "如果这个认可是免费发出的，则返回字符串“Free”")]
        public async Task<IHttpActionResult> CreateOne(string targetId, LikeTargetType targetType)
        {
            var operatorId = User.Identity.GetUserId();
            var @operator = await _userManager.FindByIdAsync(operatorId);
            if (@operator.FreeLike <= 0 && !await _coupon.CanTriggerEventAsync(operatorId, CouponEvent.发出认可))
                return Unauthorized();

            var likeId = await _cachedData.Likes.AddAsync(operatorId, targetId, targetType);

            var free = string.Empty;
            if (likeId != null)
            {
                if (targetType == LikeTargetType.Article || targetType == LikeTargetType.Activity)
                    _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
                    {
                        Type = ContentPushType.Like,
                        ContentId = likeId
                    });

                KeylolUser targetUser;
                bool notify, steamNotify;
                MessageType messageType;
                string steamNotifyText;
                object couponDescriptionForTargetUser, couponDescriptionForOperator;
                switch (targetType)
                {
                    case LikeTargetType.Article:
                        messageType = MessageType.ArticleLike;
                        var article = await _dbContext.Articles.Where(a => a.Id == targetId)
                            .Select(a => new
                            {
                                a.Id,
                                a.Title,
                                a.Author,
                                a.SidForAuthor
                            }).SingleAsync();
                        targetUser = article.Author;
                        notify = targetUser.NotifyOnArticleLiked;
                        steamNotify = targetUser.SteamNotifyOnArticleLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你的文章《{article.Title}》：\nhttps://www.keylol.com/article/{targetUser.IdCode}/{article.SidForAuthor}";
                        couponDescriptionForTargetUser = new
                        {
                            ArticleId = article.Id,
                            OperatorId = operatorId
                        };
                        couponDescriptionForOperator = new {ArticleId = article.Id};
                        break;

                    case LikeTargetType.ArticleComment:
                        messageType = MessageType.ArticleCommentLike;
                        var articleComment = await _dbContext.ArticleComments.Where(c => c.Id == targetId)
                            .Select(c => new
                            {
                                c.Id,
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
                            $"{@operator.UserName} 认可了你在《{articleComment.ArticleTitle}》下的评论：\nhttps://www.keylol.com/article/{articleComment.ArticleAuthorIdCode}/{articleComment.ArticleSidForAuthor}#{articleComment.SidForArticle}";
                        couponDescriptionForTargetUser = new
                        {
                            ArticleCommentId = articleComment.Id,
                            OperatorId = operatorId
                        };
                        couponDescriptionForOperator = new {ArticleCommentId = articleComment.Id};
                        break;

                    case LikeTargetType.Activity:
                        messageType = MessageType.ActivityLike;
                        var activity = await _dbContext.Activities.Include(a => a.Author)
                            .Where(a => a.Id == targetId).SingleAsync();
                        targetUser = activity.Author;
                        notify = targetUser.NotifyOnActivityLiked;
                        steamNotify = targetUser.SteamNotifyOnActivityLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你的动态「{PostOfficeMessageList.CollapseActivityContent(activity)}」：\nhttps://www.keylol.com/activity/{targetUser.IdCode}/{activity.SidForAuthor}";
                        couponDescriptionForTargetUser = new
                        {
                            ActivityId = activity.Id,
                            OperatorId = operatorId
                        };
                        couponDescriptionForOperator = new {ActivityId = activity.Id};
                        break;

                    case LikeTargetType.ActivityComment:
                        messageType = MessageType.ActivityCommentLike;
                        var activityComment = await _dbContext.ActivityComments.Where(c => c.Id == targetId)
                            .Select(c => new
                            {
                                c.Id,
                                c.Commentator,
                                c.SidForActivity,
                                c.Activity,
                                ActivityAuthorIdCode = c.Activity.Author.IdCode
                            }).SingleAsync();
                        targetUser = activityComment.Commentator;
                        notify = targetUser.NotifyOnCommentLiked;
                        steamNotify = targetUser.SteamNotifyOnCommentLiked;
                        steamNotifyText =
                            $"{@operator.UserName} 认可了你在「{PostOfficeMessageList.CollapseActivityContent(activityComment.Activity)}」下的评论：\nhttps://www.keylol.com/activity/{activityComment.ActivityAuthorIdCode}/{activityComment.Activity.SidForAuthor}#{activityComment.SidForActivity}";
                        couponDescriptionForTargetUser = new
                        {
                            ActivityCommentId = activityComment.Id,
                            OperatorId = operatorId
                        };
                        couponDescriptionForOperator = new {ActivityCommentId = activityComment.Id};
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                }
                if (@operator.FreeLike > 0)
                {
                    @operator.FreeLike--;
                    free = "Free";
                }
                else
                {
                    await _coupon.UpdateAsync(@operator, CouponEvent.发出认可, couponDescriptionForOperator);
                }
                await _coupon.UpdateAsync(targetUser, CouponEvent.获得认可, couponDescriptionForTargetUser);
                if (notify)
                {
                    var message = new Message
                    {
                        Type = messageType,
                        OperatorId = operatorId,
                        ReceiverId = targetUser.Id,
                        Count = await _cachedData.Likes.GetUserLikeCountAsync(targetUser.Id),
                        SecondCount = await _cachedData.Likes.GetTargetLikeCountAsync(targetId, targetType)
                    };
                    switch (targetType)
                    {
                        case LikeTargetType.Article:
                            message.ArticleId = targetId;
                            break;

                        case LikeTargetType.ArticleComment:
                            message.ArticleCommentId = targetId;
                            break;

                        case LikeTargetType.Activity:
                            message.ActivityId = targetId;
                            break;

                        case LikeTargetType.ActivityComment:
                            message.ActivityCommentId = targetId;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
                    }
                    await _cachedData.Messages.AddAsync(message);
                }
                if (steamNotify)
                {
                    await _userManager.SendSteamChatMessageAsync(targetUser, steamNotifyText);
                }
            }
            return Ok(free);
        }
    }
}