using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Utilities;
using Newtonsoft.Json;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 创建一个 Spotlight Article
        /// </summary>
        /// <param name="category">文章类型</param>
        /// <param name="dto">DTO 对象</param>
        [Route("spotlight-article")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneSpotlightArticle(SpotlightArticleStream.ArticleCategory category,
            [NotNull] CreateOrUpdateOneSpotlightArticleRequestDto dto)
        {
            var article = await _dbContext.Articles.FindAsync(dto.ArticleId);
            if (article == null)
                return this.BadRequest(nameof(dto), nameof(dto.ArticleId), Errors.NonExistent);

            _dbContext.Feeds.Add(new Models.Feed
            {
                StreamName = SpotlightArticleStream.Name(category),
                EntryType = FeedEntryType.ArticleId,
                Entry = article.Id,
                Properties = JsonConvert.SerializeObject(new SpotlightArticleStream.FeedProperties
                {
                    Title = string.IsNullOrWhiteSpace(dto.Title) || dto.Title == article.Title
                        ? null
                        : dto.Title,
                    Subtitle = string.IsNullOrWhiteSpace(dto.Subtitle) || dto.Subtitle == article.Subtitle
                        ? null
                        : dto.Subtitle
                }, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
            });

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// CreateOneSlideshowEntry request DTO
        /// </summary>
        public class CreateOrUpdateOneSpotlightArticleRequestDto
        {
            /// <summary>
            /// 文章 ID
            /// </summary>
            [Required]
            public string ArticleId { get; set; }

            /// <summary>
            /// 主标题
            /// </summary>
            [MaxLength(128)]
            public string Title { get; set; }

            /// <summary>
            /// 副标题
            /// </summary>
            [MaxLength(256)]
            public string Subtitle { get; set; }
        }
    }
}