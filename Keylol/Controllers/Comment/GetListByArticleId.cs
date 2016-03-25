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
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        public enum OrderByType
        {
            SequenceNumberForAuthor,
            LikeCount
        }

        /// <summary>
        ///     获取指定文章下的评论（被封存的文章只能作者和运维职员可见）
        /// </summary>
        /// <remarks>响应 Header 中 X-Total-Record-Count 记录了当前文章下的总评论数目</remarks>
        /// <param name="articleId">文章 ID</param>
        /// <param name="orderBy">排序字段，默认 "SequenceNumberForAuthor"</param>
        /// <param name="desc">true 表示降序，false 表示升序，默认 false</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 20</param>
        [Route]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<CommentDTO>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "文章被封存，当前登录用户无权查看评论")]
        public async Task<HttpResponseMessage> GetListByArticleId(string articleId,
            OrderByType orderBy = OrderByType.SequenceNumberForAuthor,
            bool desc = false, int skip = 0, int take = 20)
        {
            var userId = User.Identity.GetUserId();
            if (take > 50) take = 50;

            var article = await DbContext.Articles.FindAsync(articleId);
            if (article == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var staffClaim = await UserManager.GetStaffClaimAsync(userId);
            if (article.Archived != ArchivedState.None &&
                userId != article.PrincipalId && staffClaim != StaffClaim.Operator)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

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
                commentEntries.Select(entry =>
                {
                    if (entry.comment.Archived != ArchivedState.None &&
                        userId != entry.comment.CommentatorId && staffClaim != StaffClaim.Operator)
                        return new CommentDTO
                        {
                            SequenceNumberForArticle = entry.comment.SequenceNumberForArticle
                        };
                    return new CommentDTO(entry.comment)
                    {
                        Commentator = new UserDTO(entry.commentator),
                        LikeCount = entry.likeCount,
                        Liked = entry.liked,
                        Archived = entry.comment.Archived,
                        Warned = entry.comment.Warned
                    };
                }).ToList());
            var commentCount = await DbContext.Comments.Where(c => c.ArticleId == articleId).CountAsync();
            response.Headers.Add("X-Total-Record-Count", commentCount.ToString());
            return response;
        }
    }
}