using System;
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
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ArticleDto))]
        public async Task<IHttpActionResult> CreateOne([NotNull] CreateOrUpdateOneRequestDto requestDto)
        {
            var userId = User.Identity.GetUserId();
            var article = new Models.Article
            {
                AuthorId = userId,
                Title = requestDto.Title,
            };

            if (!string.IsNullOrWhiteSpace(requestDto.Subtitle))
                article.Subtitle = requestDto.Subtitle;

            var targetPoint = await _dbContext.Points.FindAsync(requestDto.TargetPointId);
            if (targetPoint == null)
                return this.BadRequest(nameof(requestDto), nameof(requestDto.TargetPointId), Errors.NonExistent);

            article.TargetPointId = targetPoint.Id;

            if (targetPoint.Type == PointType.Game || targetPoint.Type == PointType.Hardware)
            {
                article.Rating = requestDto.Rating;
                article.Pros = JsonConvert.SerializeObject(requestDto.Pros ?? new List<string>());
                article.Cons = JsonConvert.SerializeObject(requestDto.Cons ?? new List<string>());
            }
            _dbContext.Articles.Add(article);
            article.SidForAuthor =
                _dbContext.Articles.Where(a => a.AuthorId == article.AuthorId)
                    .Select(a => a.SidForAuthor)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            _dbContext.SaveChanges();
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ArticleId = article.Id
            });
//            var author = await _userManager.FindByIdAsync(userId);
//            await _coupon.Update(author, couponEvent, new {ArticleId = article.Id});
//            return Created($"article/{article.Id}", new ArticleDto(article));
            return Ok();
        }

        /// <summary>
        ///     请求 DTO（CreateOne 与 UpdateOne 共用）
        /// </summary>
        public class CreateOrUpdateOneRequestDto
        {
            /// <summary>
            ///     文章标题
            /// </summary>
            [Required]
            [MaxLength(50)]
            public string Title { get; set; }

            /// <summary>
            ///     文章副标题
            /// </summary>
            [MaxLength(50)]
            public string Subtitle { get; set; }

            /// <summary>
            ///     文章内容
            /// </summary>
            [Required]
            [MaxLength(100000)]
            public string Content { get; set; }

            /// <summary>
            ///     投稿据点 ID
            /// </summary>
            [Required]
            public string TargetPointId { get; set; }

            /// <summary>
            ///     额外投稿据点 ID 列表
            /// </summary>
            [MaxLength(10)]
            public List<string> AttachedPointIds { get; set; }

            /// <summary>
            ///     文章打出的评分
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
        }
    }
}