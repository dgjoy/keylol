using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("normal-point")]
    public class NormalPointController : KeylolApiController
    {
        public enum IdType
        {
            Id,
            IdCode
        }

        /// <summary>
        /// 取得指定据点的资料
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="includeStats">是否包含读者数和文章数，默认 false</param>
        /// <param name="idType">Id 类型，默认 "Id"</param>
        [Route("{id}")]
        [ResponseType(typeof(NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> Get(string id, bool includeStats = false, IdType idType = IdType.Id)
        {
            var pointQuery = DbContext.NormalPoints.Where(p => idType == IdType.IdCode ? p.IdCode == id : p.Id == id);
            if (includeStats)
            {
                var pointEntry = await pointQuery.Select(p => new { point = p, articleCount = p.Articles.Count, subscriberCount = p.Subscribers.Count }).SingleOrDefaultAsync();
                if (pointEntry == null)
                    return NotFound();

                return Ok(new NormalPointDTO(pointEntry.point)
                {
                    ArticleCount = pointEntry.articleCount,
                    SubscriberCount = pointEntry.subscriberCount
                });
            }

            var point = await pointQuery.SingleOrDefaultAsync();
            if (point == null)
                return NotFound();

            return Ok(new NormalPointDTO(point));
        }

        /// <summary>
        /// 根据关键字搜索对应据点
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route]
        [ResponseType(typeof(List<NormalPointDTO>))]
        public async Task<IHttpActionResult> Get(string keyword, int skip = 0, int take = 5)
        {
            if (take > 50) take = 50;
            return Ok((await DbContext.NormalPoints.SqlQuery(@"SELECT * FROM [dbo].[NormalPoints] AS [t1] INNER JOIN (
                    SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
		                UNION ALL
		                SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})
	                ) AS [t2] GROUP BY [t2].[KEY]
                ) AS [t3] ON [t1].[Id] = [t3].[KEY]
                ORDER BY [t3].[RANK] DESC
                OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync()).Select(point => new NormalPointDTO(point)));
        }

        /// <summary>
        /// 创建一个据点
        /// </summary>
        /// <param name="vm">据点相关属性</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(NormalPointVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "Only 5 uppercase letters and digits are allowed in IdCode.");
                return BadRequest(ModelState);
            }
            if (await DbContext.NormalPoints.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "IdCode is already used by others.");
                return BadRequest(ModelState);
            }

            var normalPoint = DbContext.NormalPoints.Create();
            normalPoint.IdCode = vm.IdCode;
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.EnglishName = vm.EnglishName;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Type = vm.Type;
            if (normalPoint.Type == NormalPointType.Game)
            {
                if (string.IsNullOrEmpty(vm.StoreLink))
                {
                    ModelState.AddModelError("vm.StoreLink", "Invalid store link for game point.");
                    return BadRequest(ModelState);
                }
                normalPoint.StoreLink = vm.StoreLink;
            }

            DbContext.NormalPoints.Add(normalPoint);
            await DbContext.SaveChangesAsync();

            return Created($"normal-point/{normalPoint.Id}", new NormalPointDTO(normalPoint));
        }
    }
}