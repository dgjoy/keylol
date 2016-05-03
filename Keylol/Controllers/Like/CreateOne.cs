using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

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
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权创建认可（文章或评论被封存，或者用户文券不足）")]
        public async Task<IHttpActionResult> CreateOne([NotNull] LikeCreateOneRequestDto requestDto)
        {
            var operatorId = User.Identity.GetUserId();
            var @operator = await _userManager.FindByIdAsync(operatorId);
            if (@operator.FreeLike <= 0 && !await _coupon.CanTriggerEvent(operatorId, CouponEvent.发出认可))
                return Unauthorized();

            Models.Like like;
            var free = false;
            switch (requestDto.Type)
            {
                case LikeType.ArticleLike:
                {
                    var existLike = await _dbContext.ArticleLikes.FirstOrDefaultAsync(
                        l => l.ArticleId == requestDto.TargetId && l.OperatorId == operatorId);
                    if (existLike != null)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Duplicate);
                    var article = await _dbContext.Articles.FindAsync(requestDto.TargetId);
                    if (article == null)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.NonExistent);
                    if (article.PrincipalId == operatorId)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Invalid);
                    if (article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var articleLike = _dbContext.ArticleLikes.Create();
                    articleLike.ArticleId = requestDto.TargetId;
                    like = articleLike;
                    if (!article.IgnoreNewLikes)
                    {
                        var articleAuthor = await _dbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == article.PrincipalId);

                        // 邮政中心
                        var message = _dbContext.Messages.Create();
                        message.Type = MessageType.ArticleLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = articleAuthor.Id;
                        message.ArticleId = article.Id;
                        _dbContext.Messages.Add(message);

                        // Steam 通知

                        if (articleAuthor.SteamNotifyOnArticleLiked)
                            await _userManager.SendSteamChatMessageAsync(articleAuthor,
                                $"@{@operator.UserName} 认可了你的文章 《{article.Title}》：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}");
                    }
                    await _statistics.IncreaseUserLikeCount(article.PrincipalId);
                    var author = await _userManager.FindByIdAsync(article.PrincipalId);
                    await _coupon.Update(author, CouponEvent.获得认可, new
                    {
                        ArticleId = article.Id,
                        OperatorId = operatorId
                    });
                    if (@operator.FreeLike > 0)
                    {
                        @operator.FreeLike--;
                        free = true;
                    }
                    else
                    {
                        await _coupon.Update(@operator, CouponEvent.发出认可, new {ArticleId = article.Id});
                    }
                    break;
                }

                case LikeType.CommentLike:
                {
                    var existLike = await _dbContext.CommentLikes.FirstOrDefaultAsync(
                        l => l.CommentId == requestDto.TargetId && l.OperatorId == operatorId);
                    if (existLike != null)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Duplicate);
                    var comment =
                        await
                            _dbContext.Comments.Include(c => c.Article)
                                .SingleOrDefaultAsync(c => c.Id == requestDto.TargetId);
                    if (comment == null)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.NonExistent);
                    if (comment.CommentatorId == operatorId)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetId), Errors.Invalid);
                    if (comment.Archived != ArchivedState.None || comment.Article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var commentLike = _dbContext.CommentLikes.Create();
                    commentLike.CommentId = requestDto.TargetId;
                    like = commentLike;
                    if (!comment.IgnoreNewLikes)
                    {
                        var commentAuthor = await _dbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == comment.CommentatorId);
                        var articleAuthor = await _userManager.FindByIdAsync(comment.Article.PrincipalId);

                        // 邮政中心
                        var message = _dbContext.Messages.Create();
                        message.Type = MessageType.CommentLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = commentAuthor.Id;
                        message.CommentId = comment.Id;
                        _dbContext.Messages.Add(message);

                        // Steam 通知
                        if (commentAuthor.SteamNotifyOnCommentLiked)
                            await _userManager.SendSteamChatMessageAsync(commentAuthor,
                                $"@{@operator.UserName} 认可了你在 《{comment.Article.Title}》 下的评论：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{comment.Article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                    }
                    var commentator = await _userManager.FindByIdAsync(comment.CommentatorId);
                    await _statistics.IncreaseUserLikeCount(comment.CommentatorId);
                    await _coupon.Update(commentator, CouponEvent.获得认可, new
                    {
                        CommentId = comment.Id,
                        OperatorId = operatorId
                    });
                    if (@operator.FreeLike > 0)
                    {
                        @operator.FreeLike--;
                        free = true;
                    }
                    else
                    {
                        await _coupon.Update(@operator, CouponEvent.发出认可, new {CommentId = comment.Id});
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            like.OperatorId = operatorId;
            _dbContext.Likes.Add(like);
            await _dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            return Created($"like/{like.Id}", free ? "Free" : string.Empty);
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class LikeCreateOneRequestDto
        {
            /// <summary>
            ///     认可目标 Id
            /// </summary>
            [Required]
            public string TargetId { get; set; }

            /// <summary>
            ///     认可目标类型
            /// </summary>
            public LikeType Type { get; set; }
        }
    }
}