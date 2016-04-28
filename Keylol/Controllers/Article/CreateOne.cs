using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
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
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (ArticleDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "用户文券不足")]
        public async Task<IHttpActionResult> CreateOne(ArticleCreateOrUpdateOneRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = DbContext.Articles.Create();

            article.Type = requestDto.TypeName.ToEnum<ArticleType>();
            var userId = User.Identity.GetUserId();
            var couponEvent = article.Type == ArticleType.简评 ? CouponEvent.发布简评 : CouponEvent.发布文章;
            if (!_coupon.CanTriggerEvent(userId, couponEvent))
                return Unauthorized();

            if (article.Type.AllowVote())
            {
                if (requestDto.VoteForPointId == null)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Invalid point for vote.");
                    return BadRequest(ModelState);
                }
                var voteForPoint = await DbContext.NormalPoints
                    .Include(p => p.DeveloperPoints)
                    .Include(p => p.PublisherPoints)
                    .Include(p => p.SeriesPoints)
                    .Include(p => p.GenrePoints)
                    .Include(p => p.TagPoints)
                    .SingleOrDefaultAsync(p => p.Id == requestDto.VoteForPointId);
                if (voteForPoint == null)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Invalid point for vote.");
                    return BadRequest(ModelState);
                }
                if (voteForPoint.Type != NormalPointType.Game && voteForPoint.Type != NormalPointType.Hardware)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Point for vote is not a game point.");
                    return BadRequest(ModelState);
                }
                article.VoteForPointId = voteForPoint.Id;
                article.Vote = requestDto.Vote > 5 ? 5 : (requestDto.Vote < 1 ? 1 : requestDto.Vote);

                if (requestDto.Pros == null)
                    requestDto.Pros = new List<string>();
                article.Pros = JsonConvert.SerializeObject(requestDto.Pros);

                if (requestDto.Cons == null)
                    requestDto.Cons = new List<string>();
                article.Cons = JsonConvert.SerializeObject(requestDto.Cons);

                article.AttachedPoints = voteForPoint.DeveloperPoints
                    .Concat(voteForPoint.PublisherPoints)
                    .Concat(voteForPoint.SeriesPoints)
                    .Concat(voteForPoint.GenrePoints)
                    .Concat(voteForPoint.TagPoints).ToList();
                article.AttachedPoints.Add(voteForPoint);
            }
            else
            {
                if (requestDto.AttachedPointsId == null)
                {
                    ModelState.AddModelError("vm.AttachedPointsId", "非评价类文章必须手动推送据点");
                    return BadRequest(ModelState);
                }
                if (requestDto.AttachedPointsId.Count > 50)
                {
                    ModelState.AddModelError("vm.AttachedPointsId", "推送据点数量太多");
                    return BadRequest(ModelState);
                }
                article.AttachedPoints = await DbContext.NormalPoints
                    .Where(PredicateBuilder.Contains<Models.NormalPoint, string>(requestDto.AttachedPointsId,
                        point => point.Id)).ToListAsync();
            }

            foreach (var attachedPoint in article.AttachedPoints)
            {
                attachedPoint.LastActivityTime = DateTime.Now;
            }

            article.Title = requestDto.Title;
            article.Content = requestDto.Content;

            if (article.Type == ArticleType.简评)
            {
                if (requestDto.Content.Length > 99)
                {
                    ModelState.AddModelError("vm.Content", "简评内容最多 99 字符");
                    return BadRequest(ModelState);
                }
                article.UnstyledContent = article.Content;
                article.ThumbnailImage = string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(requestDto.Summary))
                {
                    SanitizeArticle(article, true);
                }
                else
                {
                    article.UnstyledContent = requestDto.Summary;
                    SanitizeArticle(article, false);
                }
            }

            article.PrincipalId = userId;
            DbContext.Articles.Add(article);
            article.SequenceNumberForAuthor =
                DbContext.Articles.Where(a => a.PrincipalId == article.PrincipalId)
                    .Select(a => a.SequenceNumberForAuthor)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            DbContext.SaveChanges();
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ArticleId = article.Id
            });
            await _coupon.Update(userId, couponEvent, new {ArticleId = article.Id});
            return Created($"article/{article.Id}", new ArticleDto(article));
        }

        /// <summary>
        ///     请求 DTO（CreateOne 与 UpdateOne 共用）
        /// </summary>
        public class ArticleCreateOrUpdateOneRequestDto
        {
            /// <summary>
            ///     文章类型名称
            /// </summary>
            [Required]
            public string TypeName { get; set; }

            /// <summary>
            ///     文章标题
            /// </summary>
            [Required]
            public string Title { get; set; }

            /// <summary>
            ///     文章概要
            /// </summary>
            public string Summary { get; set; }

            /// <summary>
            ///     文章内容
            /// </summary>
            [Required]
            public string Content { get; set; }

            /// <summary>
            ///     文章推送到的据点 Id 列表
            /// </summary>
            public List<string> AttachedPointsId { get; set; }

            /// <summary>
            ///     文章评价的据点 Id
            /// </summary>
            public string VoteForPointId { get; set; }

            /// <summary>
            ///     文章打出的评分
            /// </summary>
            public int? Vote { get; set; }

            /// <summary>
            ///     亮点列表
            /// </summary>
            public List<string> Pros { get; set; }

            /// <summary>
            ///     缺点列表
            /// </summary>
            public List<string> Cons { get; set; }
        }
    }
}