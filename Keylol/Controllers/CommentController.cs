using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("comment")]
    public class CommentController : KeylolApiController
    {
        /// <summary>
        /// 创建一条评论
        /// </summary>
        /// <param name="vm">评论相关属性</param>
        [Route]
        [ResponseType(typeof(CommentDTO))]
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

            return Created($"comment/{comment.Id}", new CommentDTO(comment));
        }
    }
}