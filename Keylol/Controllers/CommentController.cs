using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
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
        /// <summary>
        /// 获取指定文章下的评论
        /// </summary>
        /// <param name="articleId">文章 ID</param>
        /// <param name="orderBy">排序字段，可以为 ["PublishTime", "LikeCount"] 中的一个值</param>
        /// <param name="desc">true 表示降序，false 表示升序</param>
        /// <param name="skip">起始位置</param>
        /// <param name="take">获取数量，最大 50</param>
        [Route]
        [ResponseType(typeof (List<CommentDTO>))]
        public async Task<IHttpActionResult> Get(string articleId, string orderBy = "PublishTime", bool desc = false,
            int skip = 0, int take = 30)
        {
            if (take > 50) take = 50;
            var commentsQuery = DbContext.Comments.Include(c => c.Commentator).Where(comment => comment.ArticleId == articleId);
            switch (orderBy)
            {
                case "PublishTime":
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.PublishTime)
                        : commentsQuery.OrderBy(c => c.PublishTime);
                    break;

                case "LikeCount":
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.Likes.Count)
                        : commentsQuery.OrderBy(c => c.Likes.Count);
                    break;
            }
            var comments = await commentsQuery.Skip(skip).Take(take).ToListAsync();
            return Ok(comments.Select(comment => new CommentDTO(comment)
            {
                Commentotar = new UserInCommentDTO(comment.Commentator)
            }).ToList());
        }

        /// <summary>
        /// 创建一条评论
        /// </summary>
        /// <param name="vm">评论相关属性</param>
        [Route]
        [ResponseType(typeof (CommentDTO))]
        [SwaggerResponse(400, "存在无效的输入属性")]
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