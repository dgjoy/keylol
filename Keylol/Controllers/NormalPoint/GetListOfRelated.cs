using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     取得指定游戏据点的相关据点（开发商、发行商、系列、流派、特性），这些据点在发布文章时会自动推送
        /// </summary>
        /// <param name="id">游戏据点 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}/related")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<NormalPointGetListOfRelatedResponseDto>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在或者不是游戏据点")]
        public async Task<IHttpActionResult> GetListOfRelated(string id,
            NormalPointIdentityType idType = NormalPointIdentityType.Id)
        {
            var point = await _dbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == NormalPointIdentityType.IdCode ? p.IdCode == id : p.Id == id);
            if (point == null || (point.Type != NormalPointType.Game && point.Type != NormalPointType.Hardware))
                return NotFound();

            return Ok(point.DeveloperPoints
                .Concat(point.PublisherPoints)
                .Concat(point.SeriesPoints)
                .Concat(point.GenrePoints)
                .Concat(point.TagPoints)
                .Distinct()
                .Select(p => new NormalPointGetListOfRelatedResponseDto
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    Name = GetPreferredName(p)
                }).ToList());
        }

        /// <summary>
        ///     响应 DTO
        /// </summary>
        public class NormalPointGetListOfRelatedResponseDto
        {
            /// <summary>
            ///     Id
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     识别码
            /// </summary>
            public string IdCode { get; set; }
        }
    }
}