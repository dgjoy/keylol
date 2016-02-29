using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Utilities;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        ///     重新计算评论忽略状态
        /// </summary>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [HttpPut]
        [Route("recalculate-ignored")]
        public async Task<IHttpActionResult> UpdateAllRecalculateIgnored()
        {
            foreach (var comment in await (from article in DbContext.Articles
                from comment in article.Comments
                where article.PrincipalId == comment.CommentatorId
                select comment).ToListAsync())
            {
                comment.IgnoredByArticleAuthor = true;
                comment.ReadByArticleAuthor = true;
            }
            foreach (var commentReply in await (from comment in DbContext.Comments
                from commentReply in comment.CommentRepliesAsComment
                where commentReply.Reply.CommentatorId == comment.CommentatorId
                select commentReply).ToListAsync())
            {
                commentReply.IgnoredByCommentAuthor = true;
                commentReply.ReadByCommentAuthor = true;
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}