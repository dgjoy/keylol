using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     取得指定据点的资料
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="stats">是否包含读者数和文章数，默认 false</param>
        /// <param name="votes">是否包含好评文章数和差评文章数，默认 false</param>
        /// <param name="subscribed">是否包含据点有没有被当前用户订阅的信息，默认 false</param>
        /// <param name="related">对于游戏据点，表示是否包含相关据点、别名、发行时间、App ID；对于厂商、类型据点，表示是否包含该据点旗下游戏数量。默认 false</param>
        /// <param name="coverDescription">是否包含封面图片和据点描述</param>
        /// <param name="more">如果为真，则获取据点所有额外信息，如中文索引、英文索引、商店匹配名</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (NormalPointDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> GetOneById(string id, bool stats = false, bool votes = false,
            bool subscribed = false, bool related = false, bool coverDescription = false,
            bool more = false, NormalPointIdentityType idType = NormalPointIdentityType.Id)
        {
            var point = await _dbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == NormalPointIdentityType.IdCode ? p.IdCode == id : p.Id == id);

            if (point == null)
                return NotFound();

            var pointDto = new NormalPointDto(point);

            if (stats)
            {
                var statsResult = await _dbContext.NormalPoints
                    .Where(p => p.Id == point.Id)
                    .Select(p => new {articleCount = p.Articles.Count, subscriberCount = p.Subscribers.Count})
                    .SingleAsync();
                pointDto.ArticleCount = statsResult.articleCount;
                pointDto.SubscriberCount = statsResult.subscriberCount;
            }

            if (subscribed)
            {
                var userId = User.Identity.GetUserId();
                pointDto.Subscribed = await _dbContext.Users.Where(u => u.Id == userId)
                    .SelectMany(u => u.SubscribedPoints)
                    .Select(p => p.Id)
                    .ContainsAsync(point.Id);
            }

            if (votes)
            {
                var votesResult = await _dbContext.NormalPoints
                    .Where(p => p.Id == point.Id)
                    .Select(
                        p => new
                        {
                            level1 = p.VoteByArticles.Count(a => a.Vote == 1),
                            level2 = p.VoteByArticles.Count(a => a.Vote == 2),
                            level3 = p.VoteByArticles.Count(a => a.Vote == 3),
                            level4 = p.VoteByArticles.Count(a => a.Vote == 4),
                            level5 = p.VoteByArticles.Count(a => a.Vote == 5)
                        })
                    .SingleAsync();
                pointDto.VoteStats = new Dictionary<int, int>
                {
                    [1] = votesResult.level1,
                    [2] = votesResult.level2,
                    [3] = votesResult.level3,
                    [4] = votesResult.level4,
                    [5] = votesResult.level5
                };
            }

            if (related)
            {
                switch (point.Type)
                {
                    case NormalPointType.Game:
                    case NormalPointType.Hardware:
                        pointDto.SteamAppId = point.SteamAppId;
                        pointDto.DisplayAliases = point.DisplayAliases;
                        pointDto.ReleaseDate = point.ReleaseDate;
                        pointDto.DeveloperPoints =
                            point.DeveloperPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.PublisherPoints =
                            point.PublisherPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.SeriesPoints = point.SeriesPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.GenrePoints = point.GenrePoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.TagPoints = point.TagPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.MajorPlatformPoints =
                            point.MajorPlatformPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        pointDto.MinorPlatformPoints =
                            point.MinorPlatformPoints.Select(p => new NormalPointDto(p, true)).ToList();
                        break;

                    case NormalPointType.Manufacturer:
                        pointDto.GameCountAsDeveloper = point.DeveloperForPoints.Count;
                        pointDto.GameCountAsPublisher = point.PublisherForPoints.Count;
                        break;

                    case NormalPointType.Genre:
                        pointDto.GameCountAsGenre = point.GenreForPoints.Count;
                        pointDto.GameCountAsSeries = point.SeriesForPoints.Count;
                        pointDto.GameCountAsTag = point.TagForPoints.Count;
                        break;
                }
            }

            if (coverDescription)
            {
                if (point.Type == NormalPointType.Game || point.Type == NormalPointType.Hardware)
                    pointDto.CoverImage = point.CoverImage;
                pointDto.Description = point.Description;
            }

            if (more)
            {
                pointDto.ChineseAliases = point.ChineseAliases;
                pointDto.EnglishAliases = point.EnglishAliases;
                pointDto.NameInSteamStore = string.Join("; ", point.SteamStoreNames.Select(n => n.Name));
            }

            return Ok(pointDto);
        }
    }
}