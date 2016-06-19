using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.ActivityComment
{
    public partial class ActivityCommentController
    {
        /// <summary>
        /// 创建一条动态评论
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "动态楼层号")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ActivityCommentCreateOneRequestDto requestDto)
        {
            var activity = await _dbContext.Activities.FindAsync(requestDto.ActivityId);
            if (activity == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.ActivityId), Errors.NonExistent);

            var userId = User.Identity.GetUserId();
            if (activity.Archived != ArchivedState.None &&
                userId != activity.AuthorId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            var comment = new Models.ActivityComment
            {
                ActivityId = activity.Id,
                CommentatorId = userId,
                Content = requestDto.Content,
                SidForActivity = await _dbContext.ActivityComments.Where(c => c.ActivityId == activity.Id)
                    .Select(c => c.SidForActivity)
                    .DefaultIfEmpty(0)
                    .MaxAsync() + 1
            };

            _dbContext.ActivityComments.Add(comment);
            await _dbContext.SaveChangesAsync();
            await _cachedData.ActivityComments.IncreaseActivityCommentCountAsync(activity.Id, 1);

            // TODO: 楼层回复

            return Ok(comment.SidForActivity);
        }

        /// <summary>
        /// 请求 DTO
        /// </summary>
        public class ActivityCommentCreateOneRequestDto
        {
            /// <summary>
            ///     内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            ///     动态 ID
            /// </summary>
            [Required]
            public string ActivityId { get; set; }
        }
    }
}