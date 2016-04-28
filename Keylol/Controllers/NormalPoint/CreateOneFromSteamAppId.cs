using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
using Helpers = Keylol.ServiceBase.Helpers;

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
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "使用了 fillExisted 参数但是身份不是管理员")]
        [SwaggerResponse(HttpStatusCode.NotFound, "使用了 fillExisted 参数但是拥有指定 App ID 的据点不存在")]
        public async Task<IHttpActionResult> CreateOneFromSteamAppId(int appId, bool fillExisted = false)
        {
            if (appId <= 0)
                return this.BadRequest(nameof(appId), Errors.Invalid);
            var gamePoint = await _dbContext.NormalPoints.Where(p => p.SteamAppId == appId).SingleOrDefaultAsync();
            if (fillExisted)
            {
                if (await _userManager.GetStaffClaimAsync(User.Identity.GetUserId()) != StaffClaim.Operator)
                {
                    return Unauthorized();
                }
                if (gamePoint == null)
                {
                    return NotFound();
                }
            }
            else if (gamePoint != null)
            {
                return this.BadRequest(nameof(appId), Errors.Duplicate);
            }
            else
            {
                gamePoint = _dbContext.NormalPoints.Create();
                _dbContext.NormalPoints.Add(gamePoint);
            }

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
                Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
                var dom = CQ.Create(await response.Content.ReadAsStringAsync());
                var navTexts = dom[".game_title_area .blockbg a"];
                if (!navTexts.Any() || navTexts[0].InnerText != "All Games")
                {
                    return this.BadRequest(nameof(appId), Errors.SteamAppNotGame);
                }
                if (dom[".game_area_dlc_bubble"].Any())
                {
                    return this.BadRequest(nameof(appId), Errors.SteamDlcNotSupported);
                }
                if (!fillExisted)
                {
                    gamePoint.SteamAppId = appId;
                    gamePoint.PreferredName = PreferredNameType.English;
                    gamePoint.Type = NormalPointType.Game;
                }
                if (string.IsNullOrWhiteSpace(gamePoint.BackgroundImage))
                {
                    var backgroundResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                        $"http://steamcdn-a.akamaihd.net/steam/apps/{appId}/page_bg_generated.jpg"));
                    if (backgroundResponse.IsSuccessStatusCode)
                    {
                        gamePoint.BackgroundImage = $"keylol://steam/app-backgrounds/{appId}";
                    }
                    else
                    {
                        var screenshots = dom[".highlight_strip_screenshot img"];
                        if (screenshots.Any())
                        {
                            var match = Regex.Match(screenshots[0].Attributes["src"], @"ss_([^\/]*)\.\d+x\d+\.jpg");
                            if (match.Success)
                                gamePoint.BackgroundImage =
                                    $"keylol://steam/app-backgrounds/{appId}-{match.Groups[1].Value}";
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(gamePoint.CoverImage))
                    gamePoint.CoverImage = $"keylol://steam/app-capsules/{appId}";
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
                            DateTime releaseDate;
                            gamePoint.ReleaseDate = DateTime.TryParse(values[0], out releaseDate)
                                ? releaseDate
                                : Helpers.DateTimeFromTimeStamp(0);
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
                    var relatedPoint = await _dbContext.NormalPoints
                        .Where(p => p.Type == pair.Key && p.SteamStoreNames.Select(n => n.Name).Contains(pair.Value))
                        .SingleOrDefaultAsync();
                    if (relatedPoint == null)
                    {
                        relatedPoint = _dbContext.NormalPoints.Create();
                        relatedPoint.EnglishName = pair.Value;
                        var name = await _dbContext.SteamStoreNames
                            .Where(n => n.Name == pair.Value).SingleOrDefaultAsync();
                        if (name == null)
                        {
                            name = _dbContext.SteamStoreNames.Create();
                            name.Name = pair.Value;
                            _dbContext.SteamStoreNames.Add(name);
                            await _dbContext.SaveChangesAsync();
                        }
                        relatedPoint.SteamStoreNames = new[] {name};
                        relatedPoint.Type = pair.Key;
                        relatedPoint.PreferredName = PreferredNameType.English;
                        relatedPoint.IdCode = await GenerateIdCode(pair.Value);
                        _dbContext.NormalPoints.Add(relatedPoint);
                        await _dbContext.SaveChangesAsync();
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
                gamePoint.MajorPlatformPoints.Add(
                    await _dbContext.NormalPoints.SingleAsync(p => p.IdCode == "STEAM"));
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
                await _dbContext.SaveChangesAsync();
                return Created($"normal-point/{gamePoint.Id}", new NormalPointDto(gamePoint, false, true)
                {
                    Description = gamePoint.Description,
                    SteamAppId = gamePoint.SteamAppId,
                    DisplayAliases = gamePoint.DisplayAliases,
                    CoverImage = gamePoint.CoverImage,
                    ReleaseDate = gamePoint.ReleaseDate,
                    DeveloperPoints = gamePoint.DeveloperPoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    PublisherPoints = gamePoint.PublisherPoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    SeriesPoints = gamePoint.SeriesPoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    GenrePoints = gamePoint.GenrePoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    TagPoints = gamePoint.TagPoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    MajorPlatformPoints =
                        gamePoint.MajorPlatformPoints.Select(p => new NormalPointDto(p, true)).ToList(),
                    MinorPlatformPoints =
                        gamePoint.MinorPlatformPoints.Select(p => new NormalPointDto(p, true)).ToList()
                });
            }
        }
    }
}