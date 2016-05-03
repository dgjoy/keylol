using System;
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
        /// <param name="id">文章 Id</param>
        /// <param name="requestDto">文章相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这篇文章")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdateOne(string id,
            [NotNull] ArticleCreateOrUpdateOneRequestDto requestDto)
        {
            var article = await _dbContext.Articles.Include(a => a.AttachedPoints).SingleOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            var editorId = User.Identity.GetUserId();
            if (article.PrincipalId != editorId && !User.IsInRole(KeylolRoles.Operator))
                return Unauthorized();

            var newArticleType = requestDto.TypeName.ToEnum<ArticleType>();
            if (article.Type == ArticleType.简评 != (newArticleType == ArticleType.简评))
                return Unauthorized();
            article.Type = newArticleType;

            if (article.Type.AllowVote())
            {
                if (requestDto.VoteForPointId == null)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.VoteForPointId), Errors.Required);

                var voteForPoint = await _dbContext.NormalPoints
                    .Include(p => p.DeveloperPoints)
                    .Include(p => p.PublisherPoints)
                    .Include(p => p.SeriesPoints)
                    .Include(p => p.GenrePoints)
                    .Include(p => p.TagPoints)
                    .SingleOrDefaultAsync(p => p.Id == requestDto.VoteForPointId);
                if (voteForPoint == null)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.VoteForPointId), Errors.NonExistent);

                if (voteForPoint.Type != NormalPointType.Game && voteForPoint.Type != NormalPointType.Hardware)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.VoteForPointId), Errors.Invalid);

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
                article.Vote = null;
                article.VoteForPointId = null;
                article.Pros = string.Empty;
                article.Cons = string.Empty;

                if (requestDto.AttachedPointsId == null)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.AttachedPointsId), Errors.Required);

                if (requestDto.AttachedPointsId.Count > 50)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.AttachedPointsId), Errors.TooMany);

                article.AttachedPoints = await _dbContext.NormalPoints
                    .Where(PredicateBuilder.Contains<Models.NormalPoint, string>(requestDto.AttachedPointsId,
                        point => point.Id)).ToListAsync();
            }

            foreach (var attachedPoint in article.AttachedPoints)
            {
                attachedPoint.LastActivityTime = DateTime.Now;
            }

            _dbContext.EditLogs.Add(new EditLog
            {
                ArticleId = article.Id,
                EditorId = editorId,
                OldContent = article.Content,
                OldTitle = article.Title
            });

            article.Title = requestDto.Title;
            article.Content = requestDto.Content;

            if (article.Type == ArticleType.简评)
            {
                if (requestDto.Content.Length > 99)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.Content), Errors.TooMany);

                article.UnstyledContent = article.Content;
                article.ThumbnailImage = string.Empty;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(requestDto.Summary))
                {
                    SanitizeArticle(article, true);
                }
                else
                {
                    article.UnstyledContent = requestDto.Summary;
                    SanitizeArticle(article, false);
                }
            }

            await _dbContext.SaveChangesAsync();
            _mqChannel.SendMessage(string.Empty, MqClientProvider.ImageGarageRequestQueue, new ImageGarageRequestDto
            {
                ArticleId = article.Id
            });
            return Ok();
        }
    }
}