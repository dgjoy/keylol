using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("like")]
    public class LikeController : KeylolApiController
    {
        /// <summary>
        /// 创建一个认同
        /// </summary>
        /// <param name="vm">认同相关属性</param>
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(LikeVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var operatorId = User.Identity.GetUserId();

            Like like;
            switch (vm.Type)
            {
                case LikeVM.LikeType.ArticleLike:
                    var existArticleLike =
                        await
                            DbContext.ArticleLikes.SingleOrDefaultAsync(
                                l => l.ArticleId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existArticleLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇文章重复认同。");
                        return BadRequest(ModelState);
                    }
                    var articleLike = DbContext.ArticleLikes.Create();
                    articleLike.ArticleId = vm.TargetId;
                    like = articleLike;
                    break;

                case LikeVM.LikeType.CommentLike:
                    var existCommentLike =
                        await
                            DbContext.CommentLikes.SingleOrDefaultAsync(
                                l => l.CommentId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existCommentLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇评论重复认同。");
                        return BadRequest(ModelState);
                    }
                    var commentLike = DbContext.CommentLikes.Create();
                    commentLike.CommentId = vm.TargetId;
                    like = commentLike;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            like.OperatorId = operatorId;
            DbContext.Likes.Add(like);
            await DbContext.SaveChangesAsync();
            return Created($"like/{like.Id}", "Liked!");
        }

        /// <summary>
        /// 撤销发出的认同
        /// </summary>
        /// <param name="vm">认同相关属性</param>
        [Route]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.NotFound, "当前用户并没有对指定的文章或评论发出过认同")]
        public async Task<IHttpActionResult> Delete(LikeVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var operatorId = User.Identity.GetUserId();

            switch (vm.Type)
            {
                case LikeVM.LikeType.ArticleLike:
                    var existArticleLike = await DbContext.ArticleLikes.SingleOrDefaultAsync(
                        l => l.ArticleId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existArticleLike == null)
                        return NotFound();
                    existArticleLike.Backout = true;
                    break;

                case LikeVM.LikeType.CommentLike:
                    var existCommentLike = await DbContext.CommentLikes.SingleOrDefaultAsync(
                        l => l.CommentId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existCommentLike == null)
                        return NotFound();
                    existCommentLike.Backout = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}