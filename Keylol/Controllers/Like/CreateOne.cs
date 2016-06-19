using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     创建一个认可
        /// </summary>
        /// <param name="requestDto">认可相关属性</param>
        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOne([NotNull] LikeCreateOneRequestDto requestDto)
        {
            var operatorId = User.Identity.GetUserId();
            await _cachedData.Likes.AddAsync(operatorId, requestDto.TargetId, requestDto.TargetType);
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
            // TODO: 推送通知
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

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class LikeCreateOneRequestDto
        {
            /// <summary>
            ///     目标 Id
            /// </summary>
            [Required]
            public string TargetId { get; set; }

            /// <summary>
            ///     目标类型
            /// </summary>
            public LikeTargetType TargetType { get; set; }
        }
    }
}