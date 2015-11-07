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
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("like")]
    public class LikeController : KeylolApiController
    {
        public enum MyLikeType
        {
            All,
            ArticleLike,
            CommentLike
        }

        /// <summary>
        /// 获取用户收到的认可
        /// </summary>
        /// <param name="type">要获取的认可类型，默认 "All"</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 30</param>
        [Route("my")]
        [ResponseType(typeof (List<LikeDTO>))]
        public async Task<IHttpActionResult> GetMy(MyLikeType type = MyLikeType.All, int skip = 0, int take = 30)
        {
            var userId = User.Identity.GetUserId();
            switch (type)
            {
                case MyLikeType.All:
                {
                    var likeEntries = await DbContext.ArticleLikes.Where(l =>
                        l.Article.PrincipalId == userId &&
                        l.Backout == false &&
                        l.IgnoredByTargetUser == false)
                        .Select(l => new
                        {
                            like = (Like) l,
                            article = l.Article,
                            articleAuthorIdCode = l.Article.Principal.User.IdCode,
                            @operator = l.Operator,
                            comment = (Comment) null
                        })
                        .Concat(DbContext.CommentLikes.Where(l =>
                            l.Comment.CommentatorId == userId &&
                            l.Backout == false &&
                            l.IgnoredByTargetUser == false)
                            .Select(l => new
                            {
                                like = (Like) l,
                                article = l.Comment.Article,
                                articleAuthorIdCode = l.Comment.Article.Principal.User.IdCode,
                                @operator = l.Operator,
                                comment = l.Comment
                            }))
                        .OrderByDescending(l => l.like.Time)
                        .Skip(() => skip).Take(() => take)
                        .ToListAsync();

                    var result = likeEntries.Select(e => new LikeDTO(e.like)
                    {
                        Operator = new UserDTO(e.@operator),
                        Article = new ArticleDTO
                        {
                            Id = e.article.Id,
                            Title = e.article.Title,
                            AuthorIdCode = e.articleAuthorIdCode,
                            SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                        },
                        Comment = e.comment == null ? null : new CommentDTO(e.comment, false),
                        IgnoreNew = e.comment?.IgnoreNewLikes ?? e.article.IgnoreNewLikes
                    }).ToList();

                    foreach (var entry in likeEntries)
                    {
                        entry.like.ReadByTargetUser = true;
                    }
                    await DbContext.SaveChangesAsync();

                    return Ok(result);
                }

                case MyLikeType.ArticleLike:
                {
                    var likeEntries = await DbContext.ArticleLikes.Where(l =>
                        l.Article.PrincipalId == userId &&
                        l.Backout == false &&
                        l.IgnoredByTargetUser == false)
                        .Select(l => new
                        {
                            like = (Like) l,
                            article = l.Article,
                            articleAuthorIdCode = l.Article.Principal.User.IdCode,
                            @operator = l.Operator
                        })
                        .OrderByDescending(l => l.like.Time)
                        .Skip(() => skip).Take(() => take)
                        .ToListAsync();

                    var result = likeEntries.Select(e => new LikeDTO(e.like)
                    {
                        Operator = new UserDTO(e.@operator),
                        Article = new ArticleDTO
                        {
                            Id = e.article.Id,
                            Title = e.article.Title,
                            AuthorIdCode = e.articleAuthorIdCode,
                            SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                        },
                        IgnoreNew = e.article.IgnoreNewLikes
                    }).ToList();

                    foreach (var entry in likeEntries)
                    {
                        entry.like.ReadByTargetUser = true;
                    }
                    await DbContext.SaveChangesAsync();

                    return Ok(result);
                }

                case MyLikeType.CommentLike:
                {
                    var likeEntries = await DbContext.CommentLikes.Where(l =>
                        l.Comment.CommentatorId == userId &&
                        l.Backout == false &&
                        l.IgnoredByTargetUser == false)
                        .Select(l => new
                        {
                            like = (Like) l,
                            article = l.Comment.Article,
                            articleAuthorIdCode = l.Comment.Article.Principal.User.IdCode,
                            @operator = l.Operator,
                            comment = l.Comment
                        })
                        .OrderByDescending(l => l.like.Time)
                        .Skip(() => skip).Take(() => take)
                        .ToListAsync();

                    var result = likeEntries.Select(e => new LikeDTO(e.like)
                    {
                        Operator = new UserDTO(e.@operator),
                        Article = new ArticleDTO
                        {
                            Id = e.article.Id,
                            Title = e.article.Title,
                            AuthorIdCode = e.articleAuthorIdCode,
                            SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                        },
                        Comment = new CommentDTO(e.comment, false),
                        IgnoreNew = e.comment.IgnoreNewLikes
                    }).ToList();

                    foreach (var entry in likeEntries)
                    {
                        entry.like.ReadByTargetUser = true;
                    }
                    await DbContext.SaveChangesAsync();

                    return Ok(result);
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 设置是否在提醒中忽略指定认可
        /// </summary>
        /// <param name="id">认可 ID</param>
        /// <param name="ignore">是否忽略</param>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定认可不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权对该认可进行操作")]
        public async Task<IHttpActionResult> PutIgnore(string id, bool ignore)
        {
            var like = await DbContext.Likes.FindAsync(id);
            if (like == null)
                return NotFound();

            string targetUserId = null;
            var articleLike = like as ArticleLike;
            if (articleLike != null)
            {
                targetUserId = articleLike.Article.PrincipalId;
            }
            else
            {
                var commentLike = like as CommentLike;
                if (commentLike != null)
                    targetUserId = commentLike.Comment.CommentatorId;
            }

            var userId = User.Identity.GetUserId();
            if (targetUserId != userId)
                return Unauthorized();

            like.IgnoredByTargetUser = ignore;
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 创建一个认可
        /// </summary>
        /// <param name="vm">认可相关属性</param>
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
                {
                    var existLike = await DbContext.ArticleLikes.SingleOrDefaultAsync(
                        l => l.ArticleId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇文章重复认可。");
                        return BadRequest(ModelState);
                    }
                    var article = await DbContext.Articles.FindAsync(vm.TargetId);
                    if (article == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定文章不存在。");
                        return BadRequest(ModelState);
                    }
                    if (article.PrincipalId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发表的文章。");
                        return BadRequest(ModelState);
                    }
                    var articleLike = DbContext.ArticleLikes.Create();
                    articleLike.IgnoredByTargetUser = article.IgnoreNewLikes;
                    articleLike.ArticleId = vm.TargetId;
                    like = articleLike;
                    break;
                }

                case LikeVM.LikeType.CommentLike:
                {
                    var existLike = await DbContext.CommentLikes.SingleOrDefaultAsync(
                        l => l.CommentId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇评论重复认可。");
                        return BadRequest(ModelState);
                    }
                    var comment = await DbContext.Comments.FindAsync(vm.TargetId);
                    if (comment == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定评论不存在。");
                        return BadRequest(ModelState);
                    }
                    if (comment.CommentatorId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发表的评论。");
                        return BadRequest(ModelState);
                    }
                    var commentLike = DbContext.CommentLikes.Create();
                    commentLike.IgnoredByTargetUser = comment.IgnoreNewLikes;
                    commentLike.CommentId = vm.TargetId;
                    like = commentLike;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            like.OperatorId = operatorId;
            DbContext.Likes.Add(like);
            await DbContext.SaveChangesAsync();
            return Created($"like/{like.Id}", "Liked!");
        }

        /// <summary>
        /// 撤销发出的认可
        /// </summary>
        /// <param name="targetId">目标文章或评论 ID</param>
        /// <param name="type">认可类型</param>
        [Route]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.NotFound, "当前用户并没有对指定的文章或评论发出过认可")]
        public async Task<IHttpActionResult> Delete(string targetId, LikeVM.LikeType type)
        {
            var operatorId = User.Identity.GetUserId();
            switch (type)
            {
                case LikeVM.LikeType.ArticleLike:
                    var existArticleLike = await DbContext.ArticleLikes.SingleOrDefaultAsync(
                        l => l.ArticleId == targetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existArticleLike == null)
                        return NotFound();
                    existArticleLike.Backout = true;
                    break;

                case LikeVM.LikeType.CommentLike:
                    var existCommentLike = await DbContext.CommentLikes.SingleOrDefaultAsync(
                        l => l.CommentId == targetId && l.OperatorId == operatorId && l.Backout == false);
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