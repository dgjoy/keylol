using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Services;
using Keylol.Services.Contracts;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     创建一个认可
        /// </summary>
        /// <param name="createOneDto">认可相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权创建认可（文章或评论被封存，或者用户文券不足）")]
        public async Task<IHttpActionResult> CreateOne(LikeCreateOneDto createOneDto)
        {
            if (createOneDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var operatorId = User.Identity.GetUserId();
            var @operator = await DbContext.Users.SingleAsync(u => u.Id == operatorId);
            if (@operator.FreeLike <= 0 && !_coupon.CanTriggerEvent(operatorId, CouponEvent.发出认可))
                return Unauthorized();

            Models.Like like;
            var free = false;
            switch (createOneDto.Type)
            {
                case LikeType.ArticleLike:
                {
                    var existLike = await DbContext.ArticleLikes.FirstOrDefaultAsync(
                        l => l.ArticleId == createOneDto.TargetId && l.OperatorId == operatorId);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇文章重复认可。");
                        return BadRequest(ModelState);
                    }
                    var article = await DbContext.Articles.FindAsync(createOneDto.TargetId);
                    if (article == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定文章不存在。");
                        return BadRequest(ModelState);
                    }
                    if (article.PrincipalId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发布的文章。");
                        return BadRequest(ModelState);
                    }
                    if (article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var articleLike = DbContext.ArticleLikes.Create();
                    articleLike.ArticleId = createOneDto.TargetId;
                    like = articleLike;
                    if (!article.IgnoreNewLikes)
                    {
                        var articleAuthor = await DbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == article.PrincipalId);

                        // 邮政中心
                        var message = DbContext.Messages.Create();
                        message.Type = MessageType.ArticleLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = articleAuthor.Id;
                        message.ArticleId = article.Id;
                        DbContext.Messages.Add(message);

                        // Steam 通知
                        ISteamBotCoodinatorCallback callback;
                        if (articleAuthor.SteamNotifyOnArticleLiked && articleAuthor.SteamBot.SessionId != null &&
                            SteamBotCoodinator.Clients.TryGetValue(articleAuthor.SteamBot.SessionId, out callback))
                        {
                            callback.SendMessage(articleAuthor.SteamBotId, articleAuthor.SteamId,
                                $"@{@operator.UserName} 认可了你的文章 《{article.Title}》：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}");
                        }
                    }
                    await _statistics.IncreaseUserLikeCount(article.PrincipalId);
                    await _coupon.Update(article.PrincipalId, CouponEvent.获得认可, new
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
                        await _coupon.Update(operatorId, CouponEvent.发出认可, new {ArticleId = article.Id});
                    }
                    break;
                }

                case LikeType.CommentLike:
                {
                    var existLike = await DbContext.CommentLikes.FirstOrDefaultAsync(
                        l => l.CommentId == createOneDto.TargetId && l.OperatorId == operatorId);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇评论重复认可。");
                        return BadRequest(ModelState);
                    }
                    var comment =
                        await
                            DbContext.Comments.Include(c => c.Article)
                                .SingleOrDefaultAsync(c => c.Id == createOneDto.TargetId);
                    if (comment == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定评论不存在。");
                        return BadRequest(ModelState);
                    }
                    if (comment.CommentatorId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发布的评论。");
                        return BadRequest(ModelState);
                    }
                    if (comment.Archived != ArchivedState.None || comment.Article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var commentLike = DbContext.CommentLikes.Create();
                    commentLike.CommentId = createOneDto.TargetId;
                    like = commentLike;
                    if (!comment.IgnoreNewLikes)
                    {
                        var commentAuthor = await DbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == comment.CommentatorId);
                        var articleAuthor = await DbContext.Users.SingleAsync(u => u.Id == comment.Article.PrincipalId);

                        // 邮政中心
                        var message = DbContext.Messages.Create();
                        message.Type = MessageType.CommentLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = commentAuthor.Id;
                        message.CommentId = comment.Id;
                        DbContext.Messages.Add(message);

                        // Steam 通知
                        ISteamBotCoodinatorCallback callback;
                        if (commentAuthor.SteamNotifyOnCommentLiked && commentAuthor.SteamBot.SessionId != null &&
                            SteamBotCoodinator.Clients.TryGetValue(commentAuthor.SteamBot.SessionId, out callback))
                        {
                            callback.SendMessage(commentAuthor.SteamBotId, commentAuthor.SteamId,
                                $"@{@operator.UserName} 认可了你在 《{comment.Article.Title}》 下的评论：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{comment.Article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                        }
                    }
                    await _statistics.IncreaseUserLikeCount(comment.CommentatorId);
                    await _coupon.Update(comment.CommentatorId, CouponEvent.获得认可, new
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
                        await _coupon.Update(operatorId, CouponEvent.发出认可, new {CommentId = comment.Id});
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            like.OperatorId = operatorId;
            DbContext.Likes.Add(like);
            await DbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            return Created($"like/{like.Id}", free ? "Free" : string.Empty);
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class LikeCreateOneDto
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