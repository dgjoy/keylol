using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.ServiceBase;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Point
{
    public partial class PointController
    {
        /// <summary>
        /// 为游戏打折情报提供的获取指定游戏据点信息接口
        /// </summary>
        /// <param name="appId">Steam App ID</param>
        [AllowAnonymous]
        [Route("yxdzqb")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> GetOneForYxdzqb(int appId)
        {
            var point = await _dbContext.Points.Where(p => p.SteamAppId == appId && p.Type == PointType.Game)
                .Select(p => new
                {
                    p.Id,
                    p.IdCode,
                    p.ChineseAvailability
                }).SingleOrDefaultAsync();
            if (point == null)
                return NotFound();
            var chineseAvailability = Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability);
            var article = await (from a in _dbContext.Articles
                where a.TargetPointId == point.Id && a.Archived == ArchivedState.None &&
                      a.Rejected == false && a.Rating != null
                orderby _dbContext.Likes
                    .Count(l => l.TargetId == a.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    a.SidForAuthor,
                    AuthorIdCode = a.Author.IdCode
                }).FirstOrDefaultAsync();

            return Ok(new
            {
                Link = $"https://www.keylol.com/point/{point.IdCode}",
                (await _cachedData.Points.GetRatingsAsync(point.Id)).AverageRating,
                ChineseAvailable = (chineseAvailability?.SimplifiedChinese?.Interface ?? false) ||
                                   (chineseAvailability?.TraditionalChinese?.Interface ?? false),
                ThirdPartyChineseAvailable = chineseAvailability?.ThirdPartyLinks?.Count > 0,
                ArticleLink = article == null
                    ? null
                    : $"https://www.keylol.com/article/{article.AuthorIdCode}/{article.SidForAuthor}"
            });
        }
    }
}