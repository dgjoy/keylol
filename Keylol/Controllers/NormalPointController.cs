using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
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
        /// 获取五个最近活跃的据点
        /// </summary>
        [Route("active")]
        [ResponseType(typeof (List<NormalPointDTO>))]
        public async Task<IHttpActionResult> GetActive()
        {
            return Ok((await DbContext.NormalPoints.AsNoTracking()
                .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                .ToListAsync()).Select(point => new NormalPointDTO(point)));
        }

        /// <summary>
        /// 获取每种据点类型下最近活跃的据点
        /// </summary>
        [Route("active-of-each-type")]
        [ResponseType(typeof (Dictionary<NormalPointType, List<NormalPointDTO>>))]
        public async Task<IHttpActionResult> GetActiveOfEachType()
        {
            return Ok(new Dictionary<NormalPointType, List<NormalPointDTO>>
            {
                [NormalPointType.Game] = (await DbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Game)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 10)
                    .ToListAsync()).Select(point => new NormalPointDTO(point, true)).ToList(),
                [NormalPointType.Genre] = (await DbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Genre)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                    .ToListAsync()).Select(point => new NormalPointDTO(point, true)).ToList(),
                [NormalPointType.Manufacturer] = (await DbContext.NormalPoints.AsNoTracking()
                    .Where(p => p.Type == NormalPointType.Manufacturer)
                    .OrderByDescending(p => p.LastActivityTime).Take(() => 5)
                    .ToListAsync()).Select(point => new NormalPointDTO(point, true)).ToList()
            });
        }

        /// <summary>
        /// 取得指定据点的资料
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="includeStats">是否包含读者数和文章数，默认 false</param>
        /// <param name="includeVotes">是否包含好评文章数和差评文章数，默认 false</param>
        /// <param name="includeSubscribed">是否包含据点有没有被当前用户订阅的信息，默认 false</param>
        /// <param name="includeAssociated">是否包含关联据点信息，默认 false</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [ResponseType(typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> Get(string id, bool includeStats = false, bool includeVotes = false,
            bool includeSubscribed = false, bool includeAssociated = false, IdType idType = IdType.Id)
        {
            var point = await DbContext.NormalPoints
                .Where(p => idType == IdType.IdCode ? p.IdCode == id : p.Id == id)
                .SingleOrDefaultAsync();

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
                            positiveArticleCount = p.VoteByArticles.Count(a => a.Vote == VoteType.Positive),
                            negativeArticleCount = p.VoteByArticles.Count(a => a.Vote == VoteType.Negative)
                        })
                    .SingleAsync();
                pointDTO.PositiveArticleCount = votes.positiveArticleCount;
                pointDTO.NegativeArticleCount = votes.negativeArticleCount;
            }

            if (includeAssociated)
            {
                pointDTO.AssociatedPoints = (await DbContext.NormalPoints.Where(p => p.Id == point.Id)
                    .SelectMany(p => p.AssociatedToPoints)
                    .ToListAsync()).Select(p => new NormalPointDTO(p)).ToList();
            }

            return Ok(pointDTO);
        }

        /// <summary>
        /// 根据关键字搜索对应据点
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="full">是否获取完整的据点信息，包括读者文章数，订阅状态等，默认 false</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route("keyword/{keyword}")]
        [ResponseType(typeof (List<NormalPointDTO>))]
        public async Task<HttpResponseMessage> Get(string keyword, bool full = false, int skip = 0, int take = 5)
        {
            if (take > 50) take = 50;

            if (!full)
            {
                return Request.CreateResponse(HttpStatusCode.OK, (await DbContext.NormalPoints.SqlQuery(
                    @"SELECT * FROM [dbo].[NormalPoints] AS [t1] INNER JOIN (
                        SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
		                    SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
		                    UNION ALL
		                    SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})
	                    ) AS [t2] GROUP BY [t2].[KEY]
                    ) AS [t3] ON [t1].[Id] = [t3].[KEY]
                    ORDER BY [t3].[RANK] DESC
                    OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                    $"\"{keyword}\" OR \"{keyword}*\"", skip, take).AsNoTracking().ToListAsync()).Select(
                        point => new NormalPointDTO(point)));
            }

            var points = await DbContext.Database.SqlQuery<NormalPointDTO>(@"SELECT
                [t4].[Count],
                [t4].[Id],
                [t4].[PreferredName],
                [t4].[IdCode],
                [t4].[ChineseName],
                [t4].[EnglishName],
                [t4].[AvatarImage],
                [t4].[BackgroundImage],
                [t4].[Type],
	            (SELECT
		            COUNT(1)
		            FROM [dbo].[UserPointSubscriptions]
		            WHERE [t4].[Id] = [dbo].[UserPointSubscriptions].[Point_Id]) AS [SubscriberCount],
	            CASE WHEN (EXISTS (SELECT 1 FROM [dbo].[UserPointSubscriptions] WHERE [dbo].[UserPointSubscriptions].[Point_Id] = [t4].[Id] AND [dbo].[UserPointSubscriptions].[KeylolUser_Id] = {1})) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [Subscribed],
                (SELECT
                    COUNT(1)
                    FROM  [dbo].[ArticlePointPushes]
                    INNER JOIN [dbo].[Entries] ON [dbo].[Entries].[Id] = [dbo].[ArticlePointPushes].[Article_Id]
                    WHERE ([dbo].[Entries].[Discriminator] = N'Article') AND ([t4].[Id] = [dbo].[ArticlePointPushes].[NormalPoint_Id])) AS [ArticleCount]
                FROM (SELECT
                    *,
                    COUNT(1) OVER() AS [Count]
                    FROM [dbo].[NormalPoints] AS [t1]
                    INNER JOIN (SELECT
                        [t2].[KEY],
                        SUM([t2].[RANK]) AS RANK
                        FROM (SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
                            UNION ALL
                            SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})) AS[t2]
                        GROUP BY [t2].[KEY])
                    AS [t3] ON [t1].[Id] = [t3].[KEY]
                    ORDER BY [t3].[RANK] DESC
                    OFFSET({2}) ROWS FETCH NEXT({3}) ROWS ONLY) AS [t4]",
                $"\"{keyword}\" OR \"{keyword}*\"", User.Identity.GetUserId(), skip, take).ToListAsync();

            var response = Request.CreateResponse(HttpStatusCode.OK, points);
            response.Headers.Add("X-Total-Record-Count", points.Count > 0 ? points[0].Count.ToString() : "0");
            return response;
        }

        /// <summary>
        /// 获取所有据点列表
        /// </summary>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 20</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route("list")]
        [ResponseType(typeof (List<NormalPointDTO>))]
        public async Task<HttpResponseMessage> GetList(int skip = 0, int take = 20)
        {
            if (take > 50) take = 50;
            var response = Request.CreateResponse(HttpStatusCode.OK,
                ((await DbContext.NormalPoints.OrderBy(p => p.CreateTime)
                    .Skip(() => skip).Take(() => take)
                    .Select(p => new
                    {
                        point = p,
                        articleCount = p.Articles.Count,
                        subscriberCount = p.Subscribers.Count,
                        associatedPoints = p.AssociatedToPoints
                    }).ToListAsync()).Select(entry => new NormalPointDTO(entry.point, false, true)
                    {
                        ArticleCount = entry.articleCount,
                        SubscriberCount = entry.subscriberCount,
                        AssociatedPoints = entry.associatedPoints.Select(p => new NormalPointDTO(p, true)).ToList()
                    })));
            response.Headers.Add("X-Total-Record-Count", (await DbContext.NormalPoints.CountAsync()).ToString());
            return response;
        }

        /// <summary>
        /// 创建一个据点
        /// </summary>
        /// <param name="vm">据点相关属性</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDTO))]
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
                ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                return BadRequest(ModelState);
            }
            if (await DbContext.NormalPoints.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                return BadRequest(ModelState);
            }

            var normalPoint = DbContext.NormalPoints.Create();
            normalPoint.IdCode = vm.IdCode;
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.EnglishName = vm.EnglishName;
            normalPoint.PreferredName = vm.PreferredName;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Type = vm.Type;
            normalPoint.AssociatedToPoints =
                await DbContext.NormalPoints.Where(p => vm.AssociatedPointsId.Contains(p.Id)).ToListAsync();
            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, vm, PopulateGamePointMode.Full))
            {
                return BadRequest(ModelState);
            }
            DbContext.NormalPoints.Add(normalPoint);
            await DbContext.SaveChangesAsync();

            return Created($"normal-point/{normalPoint.Id}", new NormalPointDTO(normalPoint));
        }

        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> Put(string id, NormalPointVM vm)
        {
            var normalPoint = await DbContext.NormalPoints.FindAsync(id);
            if (normalPoint == null)
                return NotFound();

            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                return BadRequest(ModelState);
            }
            if (vm.IdCode != normalPoint.IdCode &&
                await DbContext.NormalPoints.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                return BadRequest(ModelState);
            }

            normalPoint.IdCode = vm.IdCode;
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.EnglishName = vm.EnglishName;
            normalPoint.PreferredName = vm.PreferredName;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Type = vm.Type;
            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, vm, PopulateGamePointMode.ExceptCollectionProperties))
            {
                return BadRequest(ModelState);
            }
            normalPoint.AssociatedToPoints.Clear();
            normalPoint.DeveloperPoints.Clear();
            normalPoint.PublisherPoints.Clear();
            normalPoint.GenrePoints.Clear();
            normalPoint.TagPoints.Clear();
            normalPoint.MajorPlatformPoints.Clear();
            normalPoint.MinorPlatformForPoints.Clear();
            await DbContext.SaveChangesAsync();
            normalPoint.AssociatedToPoints =
                await DbContext.NormalPoints.Where(p => vm.AssociatedPointsId.Contains(p.Id)).ToListAsync();
            await PopulateGamePointAttributes(normalPoint, vm, PopulateGamePointMode.OnlyCollectionProperties);
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        private enum PopulateGamePointMode
        {
            Full,
            ExceptCollectionProperties,
            OnlyCollectionProperties
        }

        private async Task<bool> PopulateGamePointAttributes(NormalPoint normalPoint, NormalPointVM vm,
            PopulateGamePointMode populateMode)
        {
            if (populateMode != PopulateGamePointMode.OnlyCollectionProperties)
            {
                if (vm.SteamAppId == null)
                {
                    ModelState.AddModelError("vm.SteamAppId", "游戏据点的 App ID 不能为空");
                    return false;
                }
                if (vm.DisplayAliases == null)
                {
                    ModelState.AddModelError("vm.DisplayAliases", "游戏据点必须填写别名");
                    return false;
                }
                if (vm.ReleaseDate == null)
                {
                    ModelState.AddModelError("vm.ReleaseDate", "游戏据点的面世日期不能为空");
                    return false;
                }
                if (string.IsNullOrEmpty(vm.CoverImage))
                {
                    ModelState.AddModelError("vm.CoverImage", "游戏据点的封面图片不能为空");
                    return false;
                }
                if (vm.DeveloperPointsId == null)
                {
                    ModelState.AddModelError("vm.DeveloperPointsId", "游戏据点必须填写开发商据点");
                    return false;
                }
                if (vm.PublisherPointsId == null)
                {
                    ModelState.AddModelError("vm.PublisherPointsId", "游戏据点必须填写发行商据点");
                    return false;
                }
                if (vm.GenrePointsId == null)
                {
                    ModelState.AddModelError("vm.GenrePointsId", "游戏据点必须填写类型据点");
                    return false;
                }
                if (vm.TagPointsId == null)
                {
                    ModelState.AddModelError("vm.TagPointsId", "游戏据点必须填写特性据点");
                    return false;
                }
                if (vm.MajorPlatformPointsId == null)
                {
                    ModelState.AddModelError("vm.MajorPlatformPointsId", "游戏据点必须填写主要平台据点");
                    return false;
                }
                if (vm.MinorPlatformPointsId == null)
                {
                    ModelState.AddModelError("vm.MinorPlatformPointsId", "游戏据点必须填写次要平台据点");
                    return false;
                }
                normalPoint.SteamAppId = vm.SteamAppId.Value;
                normalPoint.DisplayAliases = vm.DisplayAliases;
                normalPoint.ReleaseDate = vm.ReleaseDate.Value;
                normalPoint.CoverImage = vm.CoverImage;
            }
            if (populateMode != PopulateGamePointMode.ExceptCollectionProperties)
            {
                normalPoint.DeveloperPoints =
                    await DbContext.NormalPoints.Where(p => vm.DeveloperPointsId.Contains(p.Id)).ToListAsync();
                normalPoint.PublisherPoints =
                    await DbContext.NormalPoints.Where(p => vm.PublisherPointsId.Contains(p.Id)).ToListAsync();
                normalPoint.GenrePoints =
                    await DbContext.NormalPoints.Where(p => vm.GenrePointsId.Contains(p.Id)).ToListAsync();
                normalPoint.TagPoints =
                    await DbContext.NormalPoints.Where(p => vm.TagPointsId.Contains(p.Id)).ToListAsync();
                normalPoint.MajorPlatformPoints =
                    await DbContext.NormalPoints.Where(p => vm.MajorPlatformPointsId.Contains(p.Id)).ToListAsync();
                normalPoint.MinorPlatformForPoints =
                    await DbContext.NormalPoints.Where(p => vm.MinorPlatformPointsId.Contains(p.Id)).ToListAsync();
            }
            return true;
        }
    }
}