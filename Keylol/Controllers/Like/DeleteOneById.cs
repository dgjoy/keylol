using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     撤销发出的认可
        /// </summary>
        /// <param name="targetId">目标文章或评论 ID</param>
        /// <param name="type">认可类型</param>
        [Route]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "当前用户并没有对指定的文章或评论发出过认可")]
        public async Task<IHttpActionResult> DeleteOneById(string targetId, LikeType type)
        {
            var operatorId = User.Identity.GetUserId();
            switch (type)
            {
                case LikeType.ArticleLike:
                {
                    var existLikes = await _dbContext.ArticleLikes.Where(
                        l => l.ArticleId == targetId && l.OperatorId == operatorId).ToListAsync();
                    if (existLikes.Count == 0)
                        return NotFound();
                    existLikes.ForEach(async l => { await _statistics.DecreaseUserLikeCount(l.Article.PrincipalId); });
                    _dbContext.Likes.RemoveRange(existLikes);
                    break;
                }

                case LikeType.CommentLike:
                {
                    var existLikes = await _dbContext.CommentLikes.Where(
                        l => l.CommentId == targetId && l.OperatorId == operatorId).ToListAsync();
                    if (existLikes.Count == 0)
                        return NotFound();
                    existLikes.ForEach(async l => { await _statistics.DecreaseUserLikeCount(l.Comment.CommentatorId); });
                    _dbContext.Likes.RemoveRange(existLikes);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}