using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Newtonsoft.Json;

namespace Keylol.Controllers.Feed
{
    public partial class FeedController
    {
        /// <summary>
        /// 创建一个 Slideshow Entry
        /// </summary>
        /// <param name="dto">DTO 对象</param>
        [Route("slideshow-entry")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneSlideshowEntry(
            [NotNull] CreateOrUpdateOneSlideshowEntryRequestDto dto)
        {
            if (!Helpers.IsTrustedUrl(dto.BackgroundImage))
                return this.BadRequest(nameof(dto), nameof(dto.BackgroundImage), Errors.Invalid);
            _dbContext.Feeds.Add(new Models.Feed
            {
                StreamName = SlideshowStream.Name,
                Properties = JsonConvert.SerializeObject(new SlideshowStream.FeedProperties
                {
                    Title = dto.Title,
                    Subtitle = dto.Subtitle,
                    Author = dto.Author,
                    Date = dto.Date,
                    MinorTitle = dto.MinorTitle,
                    MinorSubtitle = dto.MinorSubtitle,
                    BackgroundImage = dto.BackgroundImage,
                    Link = dto.Link
                })
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// CreateOneSlideshowEntry request DTO
        /// </summary>
        public class CreateOrUpdateOneSlideshowEntryRequestDto
        {
            /// <summary>
            /// 主标题
            /// </summary>
            [Required]
            public string Title { get; set; }

            /// <summary>
            /// 副标题
            /// </summary>
            [Required]
            public string Subtitle { get; set; }

            /// <summary>
            /// 作者
            /// </summary>
            [Required]
            public string Author { get; set; }

            /// <summary>
            /// 日期
            /// </summary>
            [Required]
            public string Date { get; set; }

            /// <summary>
            /// 次要主标题
            /// </summary>
            [Required]
            public string MinorTitle { get; set; }

            /// <summary>
            /// 次要副标题
            /// </summary>
            [Required]
            public string MinorSubtitle { get; set; }

            /// <summary>
            /// 背景图片
            /// </summary>
            [Required]
            public string BackgroundImage { get; set; }

            /// <summary>
            /// 目标链接
            /// </summary>
            [Required]
            public string Link { get; set; }
        }
    }
}