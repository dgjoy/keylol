using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CsQuery;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
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
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [ResponseType(typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> Get(string id, bool includeStats = false, bool includeVotes = false,
            bool includeSubscribed = false, IdType idType = IdType.Id)
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
                        subscriberCount = p.Subscribers.Count
                    }).ToListAsync()).Select(entry => new NormalPointDTO(entry.point, false, true)
                    {
                        ArticleCount = entry.articleCount,
                        SubscriberCount = entry.subscriberCount
                    })));
            response.Headers.Add("X-Total-Record-Count", (await DbContext.NormalPoints.CountAsync()).ToString());
            return response;
        }

        /// <summary>
        /// 根据指定 Steam App ID 创建一个据点
        /// </summary>
        /// <param name="appId"></param>
        [Route("from-app-id")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDTO))]
        public async Task<IHttpActionResult> PostFromAppId(int appId)
        {
            if (appId <= 0)
            {
                ModelState.AddModelError("appId", "无效的 App ID");
                return BadRequest(ModelState);
            }
            if (await DbContext.NormalPoints.Where(p => p.SteamAppId == appId).AnyAsync())
            {
                ModelState.AddModelError("appId", "这个游戏的据点已经存在");
                return BadRequest(ModelState);
            }
            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri("http://store.steampowered.com/"), new Cookie("birthtime", "-473410799"));
                using (var httpClientHandler = new HttpClientHandler {CookieContainer = cookieContainer})
                using (var httpClient = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromSeconds(30)})
                {
                    var picsAwaiter = httpClient.GetAsync($"https://steampics-mckay.rhcloud.com/info?apps={appId}");
                    var response =
                        await httpClient.GetAsync($"http://store.steampowered.com/app/{appId}/?l=english&cc=us");
                    response.EnsureSuccessStatusCode();
                    var dom = CQ.Create(await response.Content.ReadAsStringAsync());
                    var navTexts = dom[".game_title_area .blockbg a"];
                    if (!navTexts.Any() || navTexts[0].InnerText != "All Games")
                    {
                        throw new Exception("不是游戏 App");
                    }
                    if (dom[".game_area_dlc_bubble"].Any())
                    {
                        throw new Exception("不能是 DLC");
                    }
                    var gamePoint = DbContext.NormalPoints.Create();
                    gamePoint.SteamAppId = appId;
                    gamePoint.PreferredName = PreferredNameType.English;
                    gamePoint.Type = NormalPointType.Game;
                    gamePoint.BackgroundImage = $"keylol://steam/app-backgrounds/{appId}";
                    gamePoint.CoverImage = $"keylol://steam/app-headers/{appId}";
                    gamePoint.Description = dom[".game_description_snippet"].Text().Trim();
                    var genreNames = new List<string>();
                    var tags = dom[".popular_tags a.app_tag"].Select(child => child.InnerText.Trim()).ToList();
                    var developerNames = new List<string>();
                    var publisherNames = new List<string>();
                    foreach (var child in dom[".game_details .details_block"].First().Find("b"))
                    {
                        var key = child.InnerText.Trim();
                        var values = new List<string>();
                        if (string.IsNullOrWhiteSpace(child.NextSibling.NodeValue))
                        {
                            var current = child;
                            do
                            {
                                current = current.NextElementSibling;
                                values.Add(current.InnerText.Trim());
                            } while (current.NextSibling.NodeType == NodeType.TEXT_NODE &&
                                     current.NextSibling.NodeValue.Trim() == ",");
                        }
                        else
                        {
                            values.Add(child.NextSibling.NodeValue.Trim());
                        }
                        if (!values.Any())
                            continue;
                        switch (key)
                        {
                            case "Title:":
                                gamePoint.NameInSteamStore = values[0];
                                gamePoint.EnglishName = values[0];
                                break;

                            case "Genre:":
                                genreNames.AddRange(values);
                                break;

                            case "Developer:":
                                developerNames.AddRange(values);
                                break;

                            case "Publisher:":
                                publisherNames.AddRange(values);
                                break;

                            case "Release Date:":
                                gamePoint.ReleaseDate = DateTime.Parse(values[0]);
                                break;
                        }
                    }
                    gamePoint.IdCode = await GenerateIdCode(gamePoint.EnglishName);
                    DbContext.NormalPoints.Add(gamePoint);
                    var genrePointsMap = new Dictionary<string, NormalPoint>();
                    var manufacturerPointsMap = new Dictionary<string, NormalPoint>();
                    foreach (var pair in genreNames.Concat(tags).Distinct()
                        .Select(n => new KeyValuePair<NormalPointType, string>(NormalPointType.Genre, n))
                        .Concat(developerNames.Concat(publisherNames).Distinct()
                            .Select(n => new KeyValuePair<NormalPointType, string>(NormalPointType.Manufacturer, n))))
                    {
                        var relatedPoint = await DbContext.NormalPoints
                            .Where(p => p.Type == pair.Key && p.NameInSteamStore == pair.Value)
                            .SingleOrDefaultAsync();
                        if (relatedPoint == null)
                        {
                            relatedPoint = DbContext.NormalPoints.Create();
                            relatedPoint.EnglishName = pair.Value;
                            relatedPoint.NameInSteamStore = pair.Value;
                            relatedPoint.Type = pair.Key;
                            relatedPoint.PreferredName = PreferredNameType.English;
                            relatedPoint.IdCode = await GenerateIdCode(pair.Value);
                            DbContext.NormalPoints.Add(relatedPoint);
                        }
                        switch (pair.Key)
                        {
                            case NormalPointType.Genre:
                                genrePointsMap[pair.Value] = relatedPoint;
                                break;

                            case NormalPointType.Manufacturer:
                                manufacturerPointsMap[pair.Value] = relatedPoint;
                                break;
                        }
                    }
                    gamePoint.MajorPlatformPoints = new List<NormalPoint>
                    {
                        await DbContext.NormalPoints.SingleAsync(p => p.IdCode == "STEAM")
                    };
                    gamePoint.GenrePoints = genreNames.Select(n => genrePointsMap[n]).ToList();
                    gamePoint.TagPoints = tags.Select(n => genrePointsMap[n]).ToList();
                    gamePoint.DeveloperPoints = developerNames.Select(n => manufacturerPointsMap[n]).ToList();
                    gamePoint.PublisherPoints = publisherNames.Select(n => manufacturerPointsMap[n]).ToList();
                    response = await picsAwaiter;
                    response.EnsureSuccessStatusCode();
                    var root = JObject.Parse(await response.Content.ReadAsStringAsync());
                    if ((bool) root["success"])
                    {
                        gamePoint.AvatarImage =
                            $"keylol://steam/app-icons/{appId}-{(string) root["apps"][appId.ToString()]["common"]["icon"]}";
                    }
                    await DbContext.SaveChangesAsync();
                    return Created($"normal-point/{gamePoint.Id}", new NormalPointDTO(gamePoint));
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("error", e.ToString());
                ModelState.AddModelError("appId", "与 Steam 通讯失败，请重试");
                return BadRequest(ModelState);
            }
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
        public async Task<IHttpActionResult> PostManual(NormalPointVM vm)
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
            if (await DbContext.NormalPoints.AnyAsync(u => u.IdCode == vm.IdCode))
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
            normalPoint.Description = vm.Description;
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
            normalPoint.Description = vm.Description;
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
                if (vm.SeriesPointsId == null)
                {
                    ModelState.AddModelError("vm.SeriesPointsId", "游戏据点必须填写系列据点");
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
                normalPoint.SeriesPoints =
                    await DbContext.NormalPoints.Where(p => vm.SeriesPointsId.Contains(p.Id)).ToListAsync();
            }
            return true;
        }

        private async Task<string> GenerateIdCode(string name)
        {
            var convertedName = string.Join("",
                name.ToUpper().Where(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')));
            IEnumerable<string> possiblities;
            if (convertedName.Length < 5)
            {
                possiblities = Enumerable.Range(0, 20)
                    .Select(i =>
                        $"{convertedName}{Guid.NewGuid().ToString().Substring(0, 5 - convertedName.Length).ToUpper()}");
            }
            else
            {
                var combinations = convertedName.AllCombinations(5).Select(idCode => string.Join("", idCode));
                var randomList = Enumerable.Range(0, 20)
                    .Select(i => Guid.NewGuid().ToString().Substring(0, 5).ToUpper());
                possiblities = combinations.Concat(randomList);
            }
            foreach (var idCode in possiblities)
            {
                if (DbContext.NormalPoints.Local.All(p => p.IdCode != idCode) &&
                    await DbContext.NormalPoints.AllAsync(p => p.IdCode != idCode))
                    return idCode;
            }
            throw new Exception("无法找到可用的 IdCode");
        }
    }
}