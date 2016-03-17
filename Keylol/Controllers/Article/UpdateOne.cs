using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
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
        ///     编辑指定文章
        /// </summary>
        /// <param name="id">文章 Id</param>
        /// <param name="vm">文章相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这篇文章")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdateOne(string id, UpdateOneVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await DbContext.Articles.Include(a => a.AttachedPoints).SingleOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            var editorId = User.Identity.GetUserId();
            var editorStaffClaim = await UserManager.GetStaffClaimAsync(editorId);
            if (article.PrincipalId != editorId && editorStaffClaim != StaffClaim.Operator)
                return Unauthorized();

            var type = await DbContext.ArticleTypes.SingleOrDefaultAsync(t => t.Name == vm.TypeName);
            if (type == null)
            {
                ModelState.AddModelError("vm.TypeName", "Invalid article type.");
                return BadRequest(ModelState);
            }
            article.TypeId = type.Id;

            if (type.AllowVote)
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
                article.Vote = null;
                article.VoteForPointId = null;
                article.Pros = string.Empty;
                article.Cons = string.Empty;

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

            DbContext.EditLogs.Add(new EditLog
            {
                ArticleId = article.Id,
                EditorId = editorId,
                OldContent = article.Content,
                OldTitle = article.Title
            });

            article.Title = vm.Title;
            article.Content = vm.Content;

            if (type.Name == "简评")
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
                    await SanitizeArticle(article, true, editorStaffClaim == StaffClaim.Operator);
                }
                else
                {
                    article.UnstyledContent = vm.Summary;
                    await SanitizeArticle(article, false, editorStaffClaim == StaffClaim.Operator);
                }
            }

            await DbContext.SaveChangesAsync();
            await RedisProvider.GetInstance()
                .GetDatabase()
                .KeyDeleteAsync($"user:{article.PrincipalId}:profile.timeline");
            return Ok();
        }

        public class UpdateOneVM
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