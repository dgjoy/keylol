using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Activity
{
    public partial class ActivityController
    {
        /// <summary>
        /// 创建一条动态
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "动态 SidForAuthor")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ActivityCreateOrUpdateOneRequestDto requestDto)
        {
            var userId = User.Identity.GetUserId();
            var activity = new Models.Activity
            {
                AuthorId = userId,
                Content = requestDto.Content
            };

            if (Helpers.IsTrustedUrl(requestDto.CoverImage, false))
                activity.CoverImage = requestDto.CoverImage;

            var targetPoint = await _dbContext.Points.Where(p => p.Id == requestDto.TargetPointId)
                .Select(p => new
                {
                    p.Id,
                    p.Type
                }).SingleOrDefaultAsync();
            if (targetPoint == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetPointId), Errors.NonExistent);

            activity.TargetPointId = targetPoint.Id;
            if (targetPoint.Type == PointType.Game || targetPoint.Type == PointType.Hardware)
            {
                activity.AttachedPoints =
                    JsonConvert.SerializeObject(await (from relationship in _dbContext.PointRelationships
                        where relationship.SourcePointId == targetPoint.Id &&
                              (relationship.Relationship == PointRelationshipType.Developer ||
                               relationship.Relationship == PointRelationshipType.Manufacturer ||
                               relationship.Relationship == PointRelationshipType.Series ||
                               relationship.Relationship == PointRelationshipType.Tag)
                        select relationship.TargetPointId).Distinct()
                        .Where(id => id != targetPoint.Id).Take(10).ToListAsync());
                activity.Rating = requestDto.Rating;
            }
            else
            {
                activity.AttachedPoints = "[]";
            }

            _dbContext.Activities.Add(activity);
            activity.SidForAuthor = await _dbContext.Activities.Where(a => a.AuthorId == activity.AuthorId)
                .Select(a => a.SidForAuthor)
                .DefaultIfEmpty(0)
                .MaxAsync() + 1;
            await _dbContext.SaveChangesAsync();
            _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
            {
                Type = ContentPushType.Activity,
                ContentId = activity.Id
            });
            return Ok(activity.SidForAuthor);
        }

        /// <summary>
        ///     请求 DTO（CreateOne 与 UpdateOne 共用）
        /// </summary>
        public class ActivityCreateOrUpdateOneRequestDto
        {
            /// <summary>
            /// 内容
            /// </summary>
            [Required]
            [MaxLength(2000)]
            public string Content { get; set; }

            /// <summary>
            /// 封面图片
            /// </summary>
            [MaxLength(128)]
            public string CoverImage { get; set; }

            /// <summary>
            /// 投稿据点 ID
            /// </summary>
            [Required]
            public string TargetPointId { get; set; }

            /// <summary>
            /// 评分
            /// </summary>
            [Range(1, 5)]
            public int? Rating { get; set; }
        }
    }
}