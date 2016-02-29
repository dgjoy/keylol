using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        public enum MyLikeType
        {
            All,
            ArticleLike,
            CommentLike
        }

        /// <summary>
        ///     获取用户收到的认可
        /// </summary>
        /// <param name="type">要获取的认可类型，默认 "All"</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 30</param>
        [Route("my")]
        [HttpGet]
        [ResponseType(typeof (List<LikeDTO>))]
        public async Task<IHttpActionResult> GetListByCurrentUser(MyLikeType type = MyLikeType.All, int skip = 0,
            int take = 30)
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
                            like = (Models.Like) l,
                            article = l.Article,
                            articleAuthorIdCode = l.Article.Principal.User.IdCode,
                            @operator = l.Operator,
                            comment = (Models.Comment) null
                        })
                        .Concat(DbContext.CommentLikes.Where(l =>
                            l.Comment.CommentatorId == userId &&
                            l.Backout == false &&
                            l.IgnoredByTargetUser == false)
                            .Select(l => new
                            {
                                like = (Models.Like) l,
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
                            like = (Models.Like) l,
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
                            like = (Models.Like) l,
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
    }
}