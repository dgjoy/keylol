using System.Collections.Generic;
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

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     编辑指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="requestDto">文章相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这篇文章")]
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] ArticleCreateOrUpdateOneRequestDto requestDto)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (article.AuthorId != userId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            article.Title = requestDto.Title;
            article.Subtitle = string.IsNullOrWhiteSpace(requestDto.Subtitle) ? string.Empty : requestDto.Subtitle;
            article.Content = requestDto.Content;
            article.CoverImage = requestDto.CoverImage;
            SanitizeArticle(article);

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
                article.Rating = requestDto.Rating;
                article.Pros = JsonConvert.SerializeObject(requestDto.Pros ?? new List<string>());
                article.Cons = JsonConvert.SerializeObject(requestDto.Cons ?? new List<string>());
            }
            else
            {
                article.Rating = null;
                article.Pros = string.Empty;
                article.Cons = string.Empty;
            }

            article.ReproductionRequirement = requestDto.ReproductionRequirement == null
                ? string.Empty
                : JsonConvert.SerializeObject(requestDto.ReproductionRequirement);

            await _dbContext.SaveChangesAsync();
            var oldAttachedPoints = Helpers.SafeDeserialize<List<string>>(article.AttachedPoints) ?? new List<string>();
            if (requestDto.TargetPointId != article.TargetPointId ||
                !requestDto.AttachedPointIds.OrderBy(s => s).SequenceEqual(oldAttachedPoints.OrderBy(s => s)))
            {
                article.TargetPointId = targetPoint.Id;
                requestDto.AttachedPointIds = requestDto.AttachedPointIds.Select(pointId => pointId.Trim())
                    .Where(pointId => pointId != targetPoint.Id.Trim()).Distinct().ToList();
                article.AttachedPoints = JsonConvert.SerializeObject(requestDto.AttachedPointIds);
                await _dbContext.SaveChangesAsync();
                _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
                {
                    Type = ContentPushType.Article,
                    ContentId = article.Id
                });
            }
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ArticleId = article.Id
            });
            return Ok();
        }
    }
}