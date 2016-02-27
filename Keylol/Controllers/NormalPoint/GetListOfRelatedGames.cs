using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        public class GetListOfRelatedGamesEntryDTO
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string IdCode { get; set; }
        }

        public enum PointRelationship
        {
            Developer,
            Publisher,
            Series,
            Genre,
            Tag
        }

        /// <summary>
        /// 取得指定厂商或类型据点旗下的游戏据点
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="relationship">该厂商或类型据点与想要获取游戏据点的关系</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}/games")]
        [HttpGet]
        [ResponseType(typeof (List<GetListOfRelatedGamesEntryDTO>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在或者不是厂商或类型据点")]
        public async Task<IHttpActionResult> GetListOfRelatedGames(string id, PointRelationship relationship,
            IdType idType = IdType.Id)
        {
            var point = await DbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == IdType.IdCode ? p.IdCode == id : p.Id == id);
            if (point == null || (point.Type != NormalPointType.Manufacturer && point.Type != NormalPointType.Genre))
                return NotFound();

            IEnumerable<Models.NormalPoint> points;
            switch (relationship)
            {
                case PointRelationship.Developer:
                    points = point.DeveloperForPoints;
                    break;

                case PointRelationship.Publisher:
                    points = point.PublisherForPoints;
                    break;

                case PointRelationship.Series:
                    points = point.SeriesForPoints;
                    break;

                case PointRelationship.Genre:
                    points = point.GenreForPoints;
                    break;

                case PointRelationship.Tag:
                    points = point.TagForPoints;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null);
            }
            return Ok(points.Select(p => new GetListOfRelatedGamesEntryDTO
            {
                Id = p.Id,
                IdCode = p.IdCode,
                Name = GetPreferredName(p)
            }));
        }
    }
}