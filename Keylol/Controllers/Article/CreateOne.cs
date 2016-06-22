using System.Collections.Generic;
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

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     创建一篇文章
        /// </summary>
        /// <param name="requestDto">文章相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "文章 SidForAuthor")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "用户文券不足")]
        public async Task<IHttpActionResult> CreateOne([NotNull] ArticleCreateOrUpdateOneRequestDto requestDto)
        {
            var userId = User.Identity.GetUserId();
            if (!await _coupon.CanTriggerEventAsync(userId, CouponEvent.发布文章))
                return Unauthorized();

            var article = new Models.Article
            {
                AuthorId = userId,
                Title = requestDto.Title,
                Content = SanitizeRichText(requestDto.Content),
                CoverImage = SanitizeCoverImage(requestDto.CoverImage)
            };

            if (!string.IsNullOrWhiteSpace(requestDto.Subtitle))
                article.Subtitle = requestDto.Subtitle;

            var targetPoint = await _dbContext.Points.Where(p => p.Id == requestDto.TargetPointId)
                .Select(p => new
                {
                    p.Id,
                    p.Type
                }).SingleOrDefaultAsync();
            if (targetPoint == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetPointId), Errors.NonExistent);

            article.TargetPointId = targetPoint.Id;
            requestDto.AttachedPointIds = requestDto.AttachedPointIds.Select(id => id.Trim())
                .Where(id => id != targetPoint.Id).Distinct().ToList();
            article.AttachedPoints = JsonConvert.SerializeObject(requestDto.AttachedPointIds);

            if (targetPoint.Type == PointType.Game || targetPoint.Type == PointType.Hardware)
            {
                article.Rating = requestDto.Rating;
                article.Pros = JsonConvert.SerializeObject(requestDto.Pros ?? new List<string>());
                article.Cons = JsonConvert.SerializeObject(requestDto.Cons ?? new List<string>());
            }

            if (requestDto.ReproductionRequirement != null)
                article.ReproductionRequirement = JsonConvert.SerializeObject(requestDto.ReproductionRequirement);

            _dbContext.Articles.Add(article);
            article.SidForAuthor = await _dbContext.Articles.Where(a => a.AuthorId == article.AuthorId)
                .Select(a => a.SidForAuthor)
                .DefaultIfEmpty(0)
                .MaxAsync() + 1;
            await _dbContext.SaveChangesAsync();
            await _coupon.UpdateAsync(await _userManager.FindByIdAsync(userId), CouponEvent.发布文章,
                new {ArticleId = article.Id});
            _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
            {
                Type = ContentPushType.Article,
                ContentId = article.Id
            });
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ContentType = ImageGarageRequestContentType.Article,
                ContentId = article.Id
            });
            return Ok(article.SidForAuthor);
        }

        /// <summary>
        ///     请求 DTO（CreateOne 与 UpdateOne 共用）
        /// </summary>
        public class ArticleCreateOrUpdateOneRequestDto
        {
            /// <summary>
            ///     标题
            /// </summary>
            [Required]
            [MaxLength(50)]
            public string Title { get; set; }

            /// <summary>
            ///     副标题
            /// </summary>
            [MaxLength(50)]
            public string Subtitle { get; set; }

            /// <summary>
            ///     内容
            /// </summary>
            [Required]
            [MaxLength(100000)]
            public string Content { get; set; }

            /// <summary>
            /// 封面图片
            /// </summary>
            [Required]
            [MaxLength(128)]
            public string CoverImage { get; set; }

            /// <summary>
            ///     投稿据点 ID
            /// </summary>
            [Required]
            public string TargetPointId { get; set; }

            /// <summary>
            ///     额外投稿据点 ID 列表
            /// </summary>
            [Required]
            [MaxLength(10)]
            public List<string> AttachedPointIds { get; set; }

            /// <summary>
            ///     打出的评分
            /// </summary>
            [Range(1, 5)]
            public int? Rating { get; set; }

            /// <summary>
            ///     优点列表
            /// </summary>
            [MaxLength(3)]
            public List<string> Pros { get; set; }

            /// <summary>
            ///     缺点列表
            /// </summary>
            [MaxLength(3)]
            public List<string> Cons { get; set; }

            /// <summary>
            /// 转载要求
            /// </summary>
            public ReproductionRequirement ReproductionRequirement { get; set; }
        }
    }
}