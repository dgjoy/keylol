using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Controllers.Article;
using Keylol.Identity;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.ArticleComment
{
    public partial class ArticleCommentController
    {
        /// <summary>
        /// 更新指定文章评论
        /// </summary>
        /// <param name="id">评论 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定评论不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这则评论")]
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] ArticleCommentUpdateOneRequestDto requestDto)
        {
            var comment = await _dbContext.ArticleComments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (comment.CommentatorId != userId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            comment.Content = ArticleController.SanitizeRichText(requestDto.Content);
            comment.UnstyledContent = PlainTextFormatter.FlattenHtml(requestDto.Content, false);
            if (requestDto.ReplyToComment != null)
            {
                var replyToComment = await _dbContext.ArticleComments
                    .Where(c => c.ArticleId == comment.ArticleId && c.SidForArticle == requestDto.ReplyToComment)
                    .SingleOrDefaultAsync();
                if (replyToComment != null)
                    comment.ReplyToComment = replyToComment;
            }
            else
            {
                comment.ReplyToCommentId = null;
            }

            await _dbContext.SaveChangesAsync();
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ContentType = ImageGarageRequestContentType.ArticleComment,
                ContentId = comment.Id
            });
            return Ok();
        }

        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class ArticleCommentUpdateOneRequestDto
        {
            /// <summary>
            ///     内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            /// 回复的楼层号
            /// </summary>
            public int? ReplyToComment { get; set; }
        }
    }
}