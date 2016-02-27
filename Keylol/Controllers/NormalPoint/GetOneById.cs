using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
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
        /// <param name="includeStats">是否包含读者数和文章数，默认 false</param>
        /// <param name="includeVotes">是否包含好评文章数和差评文章数，默认 false</param>
        /// <param name="includeSubscribed">是否包含据点有没有被当前用户订阅的信息，默认 false</param>
        /// <param name="includeRelated">对于游戏据点，表示是否包含相关据点、别名、发行时间、App ID；对于厂商、类型据点，表示是否包含该据点旗下游戏数量。默认 false</param>
        /// <param name="includeCoverDescription">是否包含封面图片和据点描述</param>
        /// <param name="includeMore">如果为真，则获取据点所有额外信息，如中文索引、英文索引、商店匹配名</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [HttpGet]
        [ResponseType(typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> GetOneById(string id, bool includeStats = false, bool includeVotes = false,
            bool includeSubscribed = false, bool includeRelated = false, bool includeCoverDescription = false,
            bool includeMore = false, IdType idType = IdType.Id)
        {
            var point = await DbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == IdType.IdCode ? p.IdCode == id : p.Id == id);

            if (point == null)
                return NotFound();

            var pointDTO = new NormalPointDTO(point);

            if (includeStats)
            {
                var stats = await DbContext.NormalPoints
                    .Where(p => p.Id == point.Id)
                    .Select(p => new {articleCount = p.Articles.Count, subscriberCount = p.Subscribers.Count})
                    .SingleAsync();
                pointDTO.ArticleCount = stats.articleCount;
                pointDTO.SubscriberCount = stats.subscriberCount;
            }

            if (includeSubscribed)
            {
                var userId = User.Identity.GetUserId();
                pointDTO.Subscribed = await DbContext.Users.Where(u => u.Id == userId)
                    .SelectMany(u => u.SubscribedPoints)
                    .Select(p => p.Id)
                    .ContainsAsync(point.Id);
            }

            if (includeVotes)
            {
                var votes = await DbContext.NormalPoints
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
                pointDTO.VoteStats = new Dictionary<int, int>
                {
                    [1] = votes.level1,
                    [2] = votes.level2,
                    [3] = votes.level3,
                    [4] = votes.level4,
                    [5] = votes.level5
                };
            }

            if (includeRelated)
            {
                switch (point.Type)
                {
                    case NormalPointType.Game:
                        pointDTO.SteamAppId = point.SteamAppId;
                        pointDTO.DisplayAliases = point.DisplayAliases;
                        pointDTO.ReleaseDate = point.ReleaseDate;
                        pointDTO.DeveloperPoints =
                            point.DeveloperPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.PublisherPoints =
                            point.PublisherPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.SeriesPoints = point.SeriesPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.GenrePoints = point.GenrePoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.TagPoints = point.TagPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.MajorPlatformPoints =
                            point.MajorPlatformPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        pointDTO.MinorPlatformPoints =
                            point.MinorPlatformPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        break;

                    case NormalPointType.Manufacturer:
                        pointDTO.GameCountAsDeveloper = point.DeveloperForPoints.Count;
                        pointDTO.GameCountAsPublisher = point.PublisherForPoints.Count;
                        break;

                    case NormalPointType.Genre:
                        pointDTO.GameCountAsGenre = point.GenreForPoints.Count;
                        pointDTO.GameCountAsSeries = point.SeriesForPoints.Count;
                        pointDTO.GameCountAsTag = point.TagForPoints.Count;
                        break;
                }
            }

            if (includeCoverDescription)
            {
                if (point.Type == NormalPointType.Game)
                    pointDTO.CoverImage = point.CoverImage;
                pointDTO.Description = point.Description;
            }

            if (includeMore)
            {
                pointDTO.ChineseAliases = point.ChineseAliases;
                pointDTO.EnglishAliases = point.EnglishAliases;
                pointDTO.NameInSteamStore = point.NameInSteamStore;
            }

            return Ok(pointDTO);
        }
    }
}