using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("comment")]
    public class CommentController : KeylolApiController
    {
        public enum OrderByType
        {
            PublishTime,
            LikeCount
        }

        /// <summary>
        /// 获取指定文章下的评论
        /// </summary>
        /// <param name="articleId">文章 ID</param>
        /// <param name="orderBy">排序字段，默认 "PublishTime"</param>
        /// <param name="desc">true 表示降序，false 表示升序，默认 false</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route]
        [ResponseType(typeof (List<CommentDTO>))]
        public async Task<IHttpActionResult> Get(string articleId, OrderByType orderBy = OrderByType.PublishTime,
            bool desc = false,
            int skip = 0, int take = 30)
        {
            if (take > 50) take = 50;
            var commentsQuery = DbContext.Comments
                .Where(comment => comment.ArticleId == articleId);
            switch (orderBy)
            {
                case OrderByType.PublishTime:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.PublishTime)
                        : commentsQuery.OrderBy(c => c.PublishTime);
                    break;

                case OrderByType.LikeCount:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.Likes.Count)
                        : commentsQuery.OrderBy(c => c.Likes.Count);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null);
            }
            var commentEntries = await commentsQuery.Skip(skip).Take(take).Select(comment =>
                new
                {
                    comment,
                    likeCount = comment.Likes.Count(l => l.Backout == false),
                    commentator = comment.Commentator
                })
                .ToListAsync();
            return Ok(commentEntries.Select(entry => new CommentDTO(entry.comment)
            {
                Commentotar = new UserInCommentDTO(entry.commentator),
                LikeCount = entry.likeCount
            }).ToList());
        }

        /// <summary>
        /// 创建一条评论
        /// </summary>
        /// <param name="vm">评论相关属性</param>
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (CommentDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(CommentVM vm)
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

            var replyToComments = await DbContext.Comments.Where(c => vm.ReplyToCommentsId.Contains(c.Id)).ToListAsync();

            var comment = DbContext.Comments.Create();
            comment.ArticleId = article.Id;
            comment.CommentatorId = User.Identity.GetUserId();
            comment.Content = vm.Content;
            DbContext.Comments.Add(comment);
            await DbContext.SaveChangesAsync();
            DbContext.CommentReplies.AddRange(replyToComments.Select(c => new CommentReply
            {
                CommentId = c.Id,
                ReplyId = comment.Id
            }));
            await DbContext.SaveChangesAsync();

            return Created($"comment/{comment.Id}", new CommentDTO(comment, false));
        }
    }
}