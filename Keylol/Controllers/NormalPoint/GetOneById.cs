using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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
        /// <param name="includeStats">是否包含读者数和文章数，默认 false</param>
        /// <param name="includeVotes">是否包含好评文章数和差评文章数，默认 false</param>
        /// <param name="includeSubscribed">是否包含据点有没有被当前用户订阅的信息，默认 false</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [HttpGet]
        [ResponseType(typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> GetOneById(string id, bool includeStats = false, bool includeVotes = false,
            bool includeSubscribed = false, IdType idType = IdType.Id)
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

            return Ok(pointDTO);
        }
    }
}