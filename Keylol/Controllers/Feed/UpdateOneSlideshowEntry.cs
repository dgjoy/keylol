using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 更新一个 Slideshow Entry
        /// </summary>
        /// <param name="id">Feed ID</param>
        /// <param name="dto">DTO 对象</param>
        [Route("slideshow-entry/{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Feed 不存在")]
        public async Task<IHttpActionResult> UpdateOneSlideshowEntry(int id,
            [NotNull] CreateOrUpdateOneSlideshowEntryRequestDto dto)
        {
            var feed = await _dbContext.Feeds.FindAsync(id);
            if (feed == null || feed.StreamName != SlideshowStream.Name)
                return NotFound();

            if (!Helpers.IsTrustedUrl(dto.BackgroundImage))
                return this.BadRequest(nameof(dto), nameof(dto.BackgroundImage), Errors.Invalid);

            feed.Properties = JsonConvert.SerializeObject(new SlideshowStream.FeedProperties
            {
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                Author = dto.Author,
                Date = dto.Date,
                MinorTitle = dto.MinorTitle,
                MinorSubtitle = dto.MinorSubtitle,
                BackgroundImage = dto.BackgroundImage,
                Link = dto.Link
            });

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}