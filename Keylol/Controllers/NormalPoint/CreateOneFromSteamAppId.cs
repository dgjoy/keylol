using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using CsQuery.ExtensionMethods.Internal;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     根据指定 Steam App ID 创建一个据点
        /// </summary>
        /// <param name="appId">Steam App ID</param>
        /// <param name="fillExisted">（仅管理员可用）如果已经存在该 App ID 的据点，是否填充这个据点的资料，默认 false</param>
        [Route("from-app-id")]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "使用了 fillExisted 参数但是身份不是管理员")]
        public async Task<IHttpActionResult> CreateOneFromSteamAppId(int appId, bool fillExisted = false)
        {
            if (appId <= 0)
            {
                ModelState.AddModelError("appId", "无效的 App ID");
                return BadRequest(ModelState);
            }
            var gamePoint = await DbContext.NormalPoints.Where(p => p.SteamAppId == appId).SingleOrDefaultAsync();
            if (fillExisted)
            {
                if (await UserManager.GetStaffClaimAsync(User.Identity.GetUserId()) != StaffClaim.Operator)
                {
                    return Unauthorized();
                }
            }
            else if (gamePoint != null)
            {
                ModelState.AddModelError("appId", "这个游戏的据点已经存在");
                return BadRequest(ModelState);
            }
            else
            {
                gamePoint = DbContext.NormalPoints.Create();
                DbContext.NormalPoints.Add(gamePoint);
            }

            try
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri("http://store.steampowered.com/"), new Cookie("birthtime", "-473410799"));
                using (var httpClientHandler = new HttpClientHandler {CookieContainer = cookieContainer})
                using (var httpClient = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromSeconds(30)})
                {
                    Task<HttpResponseMessage> picsAwaiter = null;
                    if (!fillExisted)
                    {
                        picsAwaiter = httpClient.GetAsync($"https://steampics-mckay.rhcloud.com/info?apps={appId}");
                    }
                    var response =
                        await httpClient.GetAsync($"http://store.steampowered.com/app/{appId}/?l=english&cc=us");
                    response.EnsureSuccessStatusCode();
                    var dom = CQ.Create(await response.Content.ReadAsStringAsync());
                    var navTexts = dom[".game_title_area .blockbg a"];
                    if (!navTexts.Any() || navTexts[0].InnerText != "All Games")
                    {
                        ModelState.AddModelError("appId", "不是游戏");
                        return BadRequest(ModelState);
                    }
                    if (dom[".game_area_dlc_bubble"].Any())
                    {
                        ModelState.AddModelError("appId", "不能是 DLC");
                        return BadRequest(ModelState);
                    }
                    if (!fillExisted)
                    {
                        gamePoint.SteamAppId = appId;
                        gamePoint.PreferredName = PreferredNameType.English;
                        gamePoint.Type = NormalPointType.Game;
                    }
                    if (string.IsNullOrEmpty(gamePoint.BackgroundImage))
                        gamePoint.BackgroundImage = $"keylol://steam/app-backgrounds/{appId}";
                    if (string.IsNullOrEmpty(gamePoint.CoverImage))
                        gamePoint.CoverImage = $"keylol://steam/app-headers/{appId}";
                    gamePoint.Description = dom[".game_description_snippet"].Text().Trim();
                    var genreNames = new List<string>();
                    var tags = dom[".popular_tags a.app_tag"].Select(child => child.InnerText.Trim()).Take(5).ToList();
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
                                if (!fillExisted)
                                {
                                    gamePoint.EnglishName = values[0];
                                }
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
                    if (!fillExisted)
                    {
                        gamePoint.IdCode = await GenerateIdCode(gamePoint.EnglishName);
                    }
                    var genrePointsMap = new Dictionary<string, Models.NormalPoint>();
                    var manufacturerPointsMap = new Dictionary<string, Models.NormalPoint>();
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
                    if (fillExisted)
                    {
                        gamePoint.MajorPlatformPoints.Clear();
                        gamePoint.GenrePoints.Clear();
                        gamePoint.TagPoints.Clear();
                        gamePoint.DeveloperPoints.Clear();
                        gamePoint.PublisherPoints.Clear();
                    }
                    else
                    {
                        gamePoint.MajorPlatformPoints = new List<Models.NormalPoint>();
                        gamePoint.GenrePoints = new List<Models.NormalPoint>();
                        gamePoint.TagPoints = new List<Models.NormalPoint>();
                        gamePoint.DeveloperPoints = new List<Models.NormalPoint>();
                        gamePoint.PublisherPoints = new List<Models.NormalPoint>();
                    }
                    gamePoint.MajorPlatformPoints.Add(await DbContext.NormalPoints.SingleAsync(p => p.IdCode == "STEAM"));
                    gamePoint.GenrePoints.AddRange(genreNames.Select(n => genrePointsMap[n]).ToList());
                    gamePoint.TagPoints.AddRange(tags.Select(n => genrePointsMap[n]).ToList());
                    gamePoint.DeveloperPoints.AddRange(developerNames.Select(n => manufacturerPointsMap[n]).ToList());
                    gamePoint.PublisherPoints.AddRange(publisherNames.Select(n => manufacturerPointsMap[n]).ToList());
                    if (!fillExisted)
                    {
                        response = await picsAwaiter;
                        response.EnsureSuccessStatusCode();
                        var root = JObject.Parse(await response.Content.ReadAsStringAsync());
                        if ((bool) root["success"])
                        {
                            gamePoint.AvatarImage =
                                $"keylol://steam/app-icons/{appId}-{(string) root["apps"][appId.ToString()]["common"]["icon"]}";
                        }
                    }
                    await DbContext.SaveChangesAsync();
                    return Created($"normal-point/{gamePoint.Id}", new NormalPointDTO(gamePoint, false, true)
                    {
                        Description = gamePoint.Description,
                        SteamAppId = gamePoint.SteamAppId,
                        DisplayAliases = gamePoint.DisplayAliases,
                        CoverImage = gamePoint.CoverImage,
                        ReleaseDate = gamePoint.ReleaseDate,
                        DeveloperPoints = gamePoint.DeveloperPoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        PublisherPoints = gamePoint.PublisherPoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        SeriesPoints = gamePoint.SeriesPoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        GenrePoints = gamePoint.GenrePoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        TagPoints = gamePoint.TagPoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        MajorPlatformPoints =
                            gamePoint.MajorPlatformPoints.Select(p => new NormalPointDTO(p, true)).ToList(),
                        MinorPlatformPoints =
                            gamePoint.MinorPlatformPoints.Select(p => new NormalPointDTO(p, true)).ToList()
                    });
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("error", e.ToString());
                ModelState.AddModelError("appId", "与 Steam 通讯失败，请重试");
                return BadRequest(ModelState);
            }
        }
    }
}