using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Utilities;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 更新一个 Spotlight Article
        /// </summary>
        /// <param name="id">Feed ID</param>
        /// <param name="category">文章类型</param>
        /// <param name="dto">DTO 对象</param>
        [Route("spotlight-article/{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Feed 不存在")]
        public async Task<IHttpActionResult> UpdateOneSpotlightArticle(int id,
            SpotlightArticleStream.ArticleCategory category, [NotNull] CreateOrUpdateOneSpotlightArticleRequestDto dto)
        {
            var feed = await _dbContext.Feeds.FindAsync(id);
            if (feed == null || feed.StreamName != SpotlightArticleStream.Name(category))
                return NotFound();

            var article = await _dbContext.Articles.FindAsync(dto.ArticleId);
            if (article == null)
                return this.BadRequest(nameof(dto), nameof(dto.ArticleId), Errors.NonExistent);

            feed.Entry = article.Id;
            feed.Properties = JsonConvert.SerializeObject(new SpotlightArticleStream.FeedProperties
            {
                Title = string.IsNullOrWhiteSpace(dto.Title) || dto.Title == article.Title
                    ? null
                    : dto.Title,
                Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) || dto.Subtitle == article.Subtitle
                    ? null
                    : dto.Subtitle
            }, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}