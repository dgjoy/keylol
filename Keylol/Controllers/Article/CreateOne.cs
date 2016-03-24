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
using Keylol.Provider;
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
        /// <param name="vm">文章相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOne(CreateOneVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = DbContext.Articles.Create();

            article.Type = vm.TypeName.ToEnum<ArticleType>();

            if (article.Type.AllowVote())
            {
                if (vm.VoteForPointId == null)
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
                    .SingleOrDefaultAsync(p => p.Id == vm.VoteForPointId);
                if (voteForPoint == null)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Invalid point for vote.");
                    return BadRequest(ModelState);
                }
                if (voteForPoint.Type != NormalPointType.Game)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Point for vote is not a game point.");
                    return BadRequest(ModelState);
                }
                article.VoteForPointId = voteForPoint.Id;
                article.Vote = vm.Vote > 5 ? 5 : (vm.Vote < 1 ? 1 : vm.Vote);

                if (vm.Pros == null)
                    vm.Pros = new List<string>();
                article.Pros = JsonConvert.SerializeObject(vm.Pros);

                if (vm.Cons == null)
                    vm.Cons = new List<string>();
                article.Cons = JsonConvert.SerializeObject(vm.Cons);

                article.AttachedPoints = voteForPoint.DeveloperPoints
                    .Concat(voteForPoint.PublisherPoints)
                    .Concat(voteForPoint.SeriesPoints)
                    .Concat(voteForPoint.GenrePoints)
                    .Concat(voteForPoint.TagPoints).ToList();
                article.AttachedPoints.Add(voteForPoint);
            }
            else
            {
                if (vm.AttachedPointsId == null)
                {
                    ModelState.AddModelError("vm.AttachedPointsId", "非评价类文章必须手动推送据点");
                    return BadRequest(ModelState);
                }
                if (vm.AttachedPointsId.Count > 50)
                {
                    ModelState.AddModelError("vm.AttachedPointsId", "推送据点数量太多");
                    return BadRequest(ModelState);
                }
                article.AttachedPoints = await DbContext.NormalPoints
                    .Where(PredicateBuilder.Contains<Models.NormalPoint, string>(vm.AttachedPointsId,
                        point => point.Id)).ToListAsync();
            }

            foreach (var attachedPoint in article.AttachedPoints)
            {
                attachedPoint.LastActivityTime = DateTime.Now;
            }

            article.Title = vm.Title;
            article.Content = vm.Content;

            if (article.Type == ArticleType.简评)
            {
                if (vm.Content.Length > 199)
                {
                    ModelState.AddModelError("vm.Content", "简评内容最多 199 字符");
                    return BadRequest(ModelState);
                }
                article.UnstyledContent = article.Content;
                article.ThumbnailImage = string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(vm.Summary))
                {
                    SanitizeArticle(article, true);
                }
                else
                {
                    article.UnstyledContent = vm.Summary;
                    SanitizeArticle(article, false);
                }
            }

            article.PrincipalId = User.Identity.GetUserId();
            DbContext.Articles.Add(article);
            article.SequenceNumberForAuthor =
                DbContext.Articles.Where(a => a.PrincipalId == article.PrincipalId)
                    .Select(a => a.SequenceNumberForAuthor)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            await DbContext.GiveNextSequenceNumberAsync(article);
            DbContext.SaveChanges();
            MessageQueueProvider.CreateModel()
                .SendRequest(MessageQueueProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
                {
                    ArticleId = article.Id
                })
                .Close();
            return Created($"article/{article.Id}", new ArticleDTO(article));
        }

        public class CreateOneVM
        {
            [Required]
            public string TypeName { get; set; }

            [Required]
            public string Title { get; set; }

            public string Summary { get; set; }

            [Required]
            public string Content { get; set; }

            public List<string> AttachedPointsId { get; set; }

            public string VoteForPointId { get; set; }

            public int? Vote { get; set; }

            public List<string> Pros { get; set; }

            public List<string> Cons { get; set; }
        }
    }
}