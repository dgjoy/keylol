using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        public enum MyCommentType
        {
            Received,
            Sent
        }

        /// <summary>
        ///     获取用户收到或发出的评论
        /// </summary>
        /// <param name="type">要获取的评论类型，默认 "Received"</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("my")]
        [HttpGet]
        [ResponseType(typeof (List<CommentDTO>))]
        public async Task<IHttpActionResult> GetListByCurrentUser(MyCommentType type = MyCommentType.Received,
            int skip = 0,
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
                            replyToComment = (Models.Comment) null,
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
                        ReadByTargetUser = e.read
                    }).ToList();
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
    }
}