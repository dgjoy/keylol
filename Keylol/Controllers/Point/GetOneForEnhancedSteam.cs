using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Point
{
    public partial class PointController
    {
        /// <summary>
        /// 为 Enhanced Steam 提供的 Steam 商店页接口
        /// </summary>
        /// <param name="appId">Steam App ID</param>
        [AllowAnonymous]
        [Route("enhanced-steam")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> GetOneForEnhancedSteam(int appId)
        {
            var point = await _dbContext.Points.Where(p => p.SteamAppId == appId && p.Type == PointType.Game)
                .Select(p => new
                {
                    p.Id,
                    p.IdCode,
                    p.ChineseName,
                    p.ChineseAvailability
                }).SingleOrDefaultAsync();
            if (point == null)
                return NotFound();
            var article = await (from a in _dbContext.Articles
                where a.TargetPointId == point.Id && a.Archived == ArchivedState.None &&
                      a.Rejected == false && a.Rating != null
                orderby _dbContext.Likes
                    .Count(l => l.TargetId == a.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    a.Title,
                    a.Subtitle,
                    a.Content,
                    a.PublishTime,
                    a.SidForAuthor,
                    a.AuthorId,
                    AuthorIdCode = a.Author.IdCode,
                    AuthorUserName = a.Author.UserName
                }).FirstOrDefaultAsync();
            var articleSummary = article == null ? null : PlainTextFormatter.FlattenHtml(article.Content, true);
            if (articleSummary != null && articleSummary.Length > 200)
                articleSummary = articleSummary.Substring(0, 200);
            var thirdPartyLinks =
                Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability)?.ThirdPartyLinks;
            return Ok(new
            {
                Link = $"https://www.keylol.com/point/{point.IdCode}",
                ChineseName = string.IsNullOrWhiteSpace(point.ChineseName) ? null : point.ChineseName,
                (await _cachedData.Points.GetRatingsAsync(point.Id)).AverageRating,
                ChineseLocalizations = thirdPartyLinks?.Count > 0 ? thirdPartyLinks : null,
                Article = article == null
                    ? null
                    : new
                    {
                        Link = $"https://www.keylol.com/article/{article.AuthorIdCode}/{article.SidForAuthor}",
                        article.Title,
                        article.Subtitle,
                        Summary = articleSummary,
                        PublishDate = article.PublishTime.ToString("yyyy-MM-dd"),
                        Author = new
                        {
                            Link = $"https://www.keylol.com/user/{article.AuthorIdCode}",
                            UserName = article.AuthorUserName,
                            GameCount =
                                await _dbContext.UserSteamGameRecords.CountAsync(r => r.UserId == article.AuthorId),
                            PlayedTime = (await _dbContext.UserSteamGameRecords.FirstOrDefaultAsync(
                                r => r.UserId == article.AuthorId && r.SteamAppId == appId))?.TotalPlayedTime
                        }
                    }
            });
        }
    }
}