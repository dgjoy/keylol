using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private static readonly object CommentSaveLock = new object();

        public enum OrderByType
        {
            SequenceNumberForAuthor,
            LikeCount
        }

        public enum MyCommentType
        {
            Received,
            Sent
        }

        /// <summary>
        /// 获取指定文章下的评论
        /// </summary>
        /// <remarks>响应 Header 中 X-Total-Record-Count 记录了当前文章下的总评论数目</remarks>
        /// <param name="articleId">文章 ID</param>
        /// <param name="orderBy">排序字段，默认 "SequenceNumberForAuthor"</param>
        /// <param name="desc">true 表示降序，false 表示升序，默认 false</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 20</param>
        [Route]
        [ResponseType(typeof (List<CommentDTO>))]
        public async Task<HttpResponseMessage> Get(string articleId,
            OrderByType orderBy = OrderByType.SequenceNumberForAuthor,
            bool desc = false, int skip = 0, int take = 20)
        {
            var userId = User.Identity.GetUserId();
            if (take > 50) take = 50;
            var commentsQuery = DbContext.Comments.AsNoTracking()
                .Where(comment => comment.ArticleId == articleId);
            switch (orderBy)
            {
                case OrderByType.SequenceNumberForAuthor:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.SequenceNumberForArticle)
                        : commentsQuery.OrderBy(c => c.SequenceNumberForArticle);
                    break;

                case OrderByType.LikeCount:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.Likes.Count(l => l.Backout == false))
                        : commentsQuery.OrderBy(c => c.Likes.Count(l => l.Backout == false));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null);
            }
            var commentEntries = await commentsQuery.Skip(() => skip).Take(() => take).Select(comment =>
                new
                {
                    comment,
                    likeCount = comment.Likes.Count(l => l.Backout == false),
                    liked = comment.Likes.Any(l => l.OperatorId == userId && l.Backout == false),
                    commentator = comment.Commentator
                })
                .ToListAsync();
            var response = Request.CreateResponse(HttpStatusCode.OK,
                commentEntries.Select(entry => new CommentDTO(entry.comment)
                {
                    Commentator = new UserDTO(entry.commentator),
                    LikeCount = entry.likeCount,
                    Liked = entry.liked
                }).ToList());
            var commentCount = await DbContext.Comments.Where(c => c.ArticleId == articleId).CountAsync();
            response.Headers.Add("X-Total-Record-Count", commentCount.ToString());
            return response;
        }

        /// <summary>
        /// 获取用户收到或发出的评论
        /// </summary>
        /// <param name="type">要获取的评论类型，默认 "Received"</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("my")]
        public async Task<IHttpActionResult> Get(MyCommentType type = MyCommentType.Received, int skip = 0,
            int take = 30)
        {
            if (take > 50) take = 50;
            var userId = User.Identity.GetUserId();
            switch (type)
            {
                case MyCommentType.Received:
                {
                    var commentEntries = await DbContext.Comments.Where(
                        c => c.Article.PrincipalId == userId && c.IgnoredByArticleAuthor == false)
                        .Select(c => new
                        {
                            article = c.Article,
                            articleAuthorIdCode = c.Article.Principal.User.IdCode,
                            comment = c,
                            commentator = c.Commentator,
                            commentReply = (CommentReply) null,
                            read = c.ReadByArticleAuthor,
                            replyToComment = (Comment) null,
                            priority = 0
                        })
                        .Concat(DbContext.CommentReplies.Where(
                            r => r.Comment.CommentatorId == userId && r.IgnoredByCommentAuthor == false)
                            .Select(r => new
                            {
                                article = r.Reply.Article,
                                articleAuthorIdCode = r.Reply.Article.Principal.User.IdCode,
                                comment = r.Reply,
                                commentator = r.Reply.Commentator,
                                commentReply = r,
                                read = r.ReadByCommentAuthor,
                                replyToComment = r.Comment,
                                priority = 1
                            }))
                        .GroupBy(e => e.comment)
                        .SelectMany(g => g.Where(e => e.priority == g.Max(ee => ee.priority)))
                        .OrderByDescending(e => e.comment.PublishTime)
                        .Skip(() => skip).Take(() => take).ToListAsync();
                    var result = commentEntries.Select(e => new CommentDTO(e.comment, true, 128)
                    {
                        Commentator = new UserDTO(e.commentator),
                        Article = new ArticleDTO
                        {
                            Title = e.article.Title,
                            AuthorIdCode = e.articleAuthorIdCode,
                            SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                        },
                        ReplyToComment = e.replyToComment == null ? null : new CommentDTO(e.replyToComment, true, 32),
                        Read = e.read
                    });
                    foreach (var entry in commentEntries)
                    {
                        if (entry.commentReply == null)
                        {
                            entry.comment.ReadByArticleAuthor = true;
                        }
                        else
                        {
                            entry.commentReply.ReadByCommentAuthor = true;
                            if (entry.article.PrincipalId == userId)
                                entry.comment.ReadByArticleAuthor = true;
                        }
                    }
                    await DbContext.SaveChangesAsync();
                    return Ok(result);
                }
                case MyCommentType.Sent:
                {
                    return Ok((await DbContext.Comments.Where(c => c.CommentatorId == userId)
                        .OrderByDescending(c => c.PublishTime)
                        .Skip(() => skip).Take(() => take)
                        .Select(c => new
                        {
                            article = c.Article,
                            articleAuthorIdCode = c.Article.Principal.User.IdCode,
                            comment = c,
                            replyToMultipleUser = c.CommentRepliesAsReply.Count > 1,
                            replyToUser = c.CommentRepliesAsReply.Count > 0
                                ? c.CommentRepliesAsReply.FirstOrDefault().Comment.Commentator
                                : null,
                            replyToComment = c.CommentRepliesAsReply.Count > 0
                                ? c.CommentRepliesAsReply.FirstOrDefault().Comment
                                : null
                        }).ToListAsync()).Select(e => new CommentDTO(e.comment, true, 128)
                        {
                            Article = new ArticleDTO
                            {
                                Title = e.article.Title,
                                AuthorIdCode = e.articleAuthorIdCode,
                                SequenceNumberForAuthor = e.article.SequenceNumberForAuthor
                            },
                            ReplyToMultipleUser = e.replyToMultipleUser,
                            ReplyToUser = e.replyToMultipleUser
                                ? null
                                : (e.replyToUser == null ? null : new UserDTO(e.replyToUser)),
                            ReplyToComment = e.replyToMultipleUser
                                ? null
                                : (e.replyToComment == null ? null : new CommentDTO(e.replyToComment, true, 32))
                        }));
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
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

            var replyToComments = await DbContext.Comments
                .Where(c => c.ArticleId == article.Id && vm.ReplyToCommentsSN.Contains(c.SequenceNumberForArticle))
                .ToListAsync();

            var comment = DbContext.Comments.Create();
            DbContext.Comments.Add(comment);
            comment.ArticleId = article.Id;
            comment.CommentatorId = User.Identity.GetUserId();
            comment.Content = vm.Content;
            lock (CommentSaveLock)
            {
                comment.SequenceNumberForArticle = (DbContext.Comments.Where(c => c.ArticleId == article.Id)
                    .Select(c => c.SequenceNumberForArticle)
                    .DefaultIfEmpty(0)
                    .Max()) + 1;
                DbContext.SaveChanges();
            }
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