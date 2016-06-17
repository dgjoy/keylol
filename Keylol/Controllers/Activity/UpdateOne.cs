using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
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
        /// 编辑指定动态
        /// </summary>
        /// <param name="id">动态 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定动态不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这条动态")]
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] ActivityCreateOrUpdateOneRequestDto requestDto)
        {
            var activity = await _dbContext.Activities.FindAsync(id);
            if (activity == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (activity.AuthorId != userId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            activity.Content = requestDto.Content;
            activity.CoverImage = Helpers.IsTrustedUrl(requestDto.CoverImage, false)
                ? requestDto.CoverImage
                : string.Empty;

            var targetPoint = await _dbContext.Points.Where(p => p.Id == requestDto.TargetPointId)
                .Select(p => new
                {
                    p.Id,
                    p.Type
                }).SingleOrDefaultAsync();
            if (targetPoint == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetPointId), Errors.NonExistent);

            if (targetPoint.Type == PointType.Game || targetPoint.Type == PointType.Hardware)
            {
                activity.Rating = requestDto.Rating;
            }
            else
            {
                activity.Rating = null;
            }

            await _dbContext.SaveChangesAsync();
            if (requestDto.TargetPointId != activity.TargetPointId)
            {
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
                            .Where(pointId => pointId != targetPoint.Id).Take(10).ToListAsync());
                }
                else
                {
                    activity.AttachedPoints = "[]";
                }
                await _dbContext.SaveChangesAsync();
                _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
                {
                    Type = ContentPushType.Activity,
                    ContentId = activity.Id
                });
            }
            return Ok();
        }
    }
}