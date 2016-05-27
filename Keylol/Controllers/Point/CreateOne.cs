using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Keylol.Controllers.Point
{
    public partial class PointController
    {
        /// <summary>
        /// 创建一个据点
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOne([NotNull] CreateOneRequestDto requestDto)
        {
            requestDto.IdCode = requestDto.IdCode.ToUpper();
            if (!Regex.IsMatch(requestDto.IdCode, @"^[A-Z0-9]{5}$"))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.IdCode), Errors.Invalid);

            if (await _dbContext.Points.AnyAsync(p => p.IdCode == requestDto.IdCode))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.IdCode), Errors.Duplicate);

            if (!Helpers.IsTrustedUrl(requestDto.HeaderImage))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.HeaderImage),
                    Errors.HeaderImageUntrusted);

            if (!Helpers.IsTrustedUrl(requestDto.AvatarImage))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.HeaderImage),
                    Errors.AvatarImageUntrusted);

            switch (requestDto.Type)
            {
                case CreateOneRequestDto.CreateType.SteamGame:
                {
                    if (requestDto.SteamAppId == null || requestDto.SteamAppId <= 0)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.Invalid);

                    if (await _dbContext.Points.AnyAsync(p => p.SteamAppId == requestDto.SteamAppId))
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.Duplicate);

                    try
                    {
                        var cookieContainer = new CookieContainer();
                        cookieContainer.Add(new Uri("http://store.steampowered.com/"),
                            new Cookie("birthtime", "-473410799"));
                        var request =
                            WebRequest.CreateHttp(
                                $"http://store.steampowered.com/app/{requestDto.SteamAppId}/?l=english&cc=us");
                        var picsRequest =
                            WebRequest.CreateHttp(
                                $"https://steampics-mckay.rhcloud.com/info?apps={requestDto.SteamAppId}");
                        picsRequest.AutomaticDecompression =
                            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                        picsRequest.CookieContainer = request.CookieContainer = cookieContainer;
                        picsRequest.Timeout = request.Timeout = 30000;
                        var picsAwaiter = picsRequest.GetResponseAsync();
                        using (var response = await request.GetResponseAsync())
                        {
                            var rs = response.GetResponseStream();
                            if (rs == null)
                                throw new Exception();
                            Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
                            var dom = CQ.Create(rs);
                            var navTexts = dom[".game_title_area .blockbg a"];

                            if (!navTexts.Any() || (navTexts[0].InnerText != "All Games"))
                                return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.Invalid);

                            if (dom[".game_area_dlc_bubble"].Any())
                                return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.Invalid);

                            var point = new Models.Point
                            {
                                Type = PointType.Game,
                                IdCode = requestDto.IdCode,
                                SteamAppId = requestDto.SteamAppId,
                                EnglishName = requestDto.EnglishName,
                                TitleCoverImage = $"keylol://steam/app-headers/{requestDto.SteamAppId}"
                            };

                            if (!string.IsNullOrWhiteSpace(requestDto.ChineseName))
                                point.ChineseName = requestDto.ChineseName;

                            var genreNames = new List<string>();
                            var tags = dom[".popular_tags a.app_tag"].Select(child => child.InnerText.Trim())
                                .Except(_tagBlacklist).Take(5).ToList();
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
                                        point.ReleaseDate = DateTime.TryParse(values[0], out releaseDate)
                                            ? releaseDate
                                            : Helpers.DateTimeFromTimeStamp(0);
                                        break;
                                }
                            }

                            var screenshots = dom[".highlight_strip_screenshot img"];
                            var media = new List<PointMedia>(screenshots.Length);
                            foreach (var screenshot in screenshots)
                            {
                                var match = Regex.Match(screenshot.Attributes["src"], @"ss_([^\/]*)\.\d+x\d+\.jpg");
                                if (!match.Success) continue;
                                var link =
                                    $"keylol://steam/app-screenshots/{requestDto.SteamAppId}-{match.Groups[1].Value}";
                                if (string.IsNullOrWhiteSpace(point.HeaderImage))
                                {
                                    point.HeaderImage = link;
                                }
                                else if (string.IsNullOrWhiteSpace(point.MediaHeaderImage))
                                {
                                    point.MediaHeaderImage = link;
                                }
                                media.Add(new PointMedia
                                {
                                    Type = PointMedia.MediaType.Screenshot,
                                    Link = link
                                });
                            }
                            point.Media = JsonConvert.SerializeObject(media,
                                new JsonSerializerSettings
                                {
                                    Converters = new List<JsonConverter> {new StringEnumConverter()}
                                });

                            _dbContext.Points.Add(point);
                            await _dbContext.SaveChangesAsync();

                            var categoryPointsMap = new Dictionary<string, string>();
                            var vendorPointsMap = new Dictionary<string, string>();
                            foreach (var pair in genreNames.Concat(tags).Distinct()
                                .Select(p => new KeyValuePair<PointType, string>(PointType.Category, p))
                                .Concat(developerNames.Concat(publisherNames).Distinct()
                                    .Select(p => new KeyValuePair<PointType, string>(PointType.Vendor, p))))
                            {
                                var relatedPointId = await _dbContext.Points
                                    .Where(p => p.Type == pair.Key &&
                                                p.SteamStoreNames.Select(n => n.Name).Contains(pair.Value))
                                    .Select(p => p.Id)
                                    .SingleOrDefaultAsync();
                                if (relatedPointId == null)
                                {
                                    var relatedPoint = new Models.Point
                                    {
                                        Type = pair.Key,
                                        IdCode = await GenerateIdCode(pair.Value),
                                        EnglishName = pair.Value
                                    };
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
                                    _dbContext.Points.Add(relatedPoint);
                                    await _dbContext.SaveChangesAsync();
                                    relatedPointId = relatedPoint.Id;
                                }
                                switch (pair.Key)
                                {
                                    case PointType.Category:
                                        categoryPointsMap[pair.Value] = relatedPointId;
                                        break;

                                    case PointType.Vendor:
                                        vendorPointsMap[pair.Value] = relatedPointId;
                                        break;
                                }
                            }

                            _dbContext.PointRelationships.Add(new PointRelationship
                            {
                                Relationship = PointRelationshipType.Platform,
                                SourcePointId = point.Id,
                                TargetPointId = await _dbContext.Points.Where(p => p.IdCode == "STEAM")
                                    .Select(p => p.Id).SingleAsync()
                            });
                            _dbContext.PointRelationships.AddRange(genreNames.Select(n => new PointRelationship
                            {
                                Relationship = PointRelationshipType.Genre,
                                SourcePointId = point.Id,
                                TargetPointId = categoryPointsMap[n]
                            }));
                            _dbContext.PointRelationships.AddRange(tags.Select(n => new PointRelationship
                            {
                                Relationship = PointRelationshipType.Tag,
                                SourcePointId = point.Id,
                                TargetPointId = categoryPointsMap[n]
                            }));
                            _dbContext.PointRelationships.AddRange(developerNames.Select(n => new PointRelationship
                            {
                                Relationship = PointRelationshipType.Developer,
                                SourcePointId = point.Id,
                                TargetPointId = vendorPointsMap[n]
                            }));
                            _dbContext.PointRelationships.AddRange(publisherNames.Select(n => new PointRelationship
                            {
                                Relationship = PointRelationshipType.Publisher,
                                SourcePointId = point.Id,
                                TargetPointId = vendorPointsMap[n]
                            }));
                            await _dbContext.SaveChangesAsync();

                            using (var picsResponse = await picsAwaiter)
                            {
                                var picsRs = picsResponse.GetResponseStream();
                                if (picsRs == null)
                                    throw new Exception();
                                var sr = new StreamReader(picsRs);
                                var picsRoot = JToken.Parse(await sr.ReadToEndAsync());
                                if (!(bool) picsRoot["success"])
                                    throw new Exception();
                                point.ThumbnailImage =
                                    $"keylol://steam/app-thumbnails/{requestDto.SteamAppId}-{(string) picsRoot["apps"][requestDto.SteamAppId.ToString()]["common"]["logo"]}";
                                point.AvatarImage =
                                    $"keylol://steam/app-icons/{requestDto.SteamAppId}-{(string) picsRoot["apps"][requestDto.SteamAppId.ToString()]["common"]["icon"]}";
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.NetworkError);
                    }
                    break;
                }

                case CreateOneRequestDto.CreateType.OtherGame:
                {
                    if (string.IsNullOrWhiteSpace(requestDto.HeaderImage))
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.HeaderImage), Errors.Required);

                    if (string.IsNullOrWhiteSpace(requestDto.AvatarImage))
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.AvatarImage), Errors.Required);

                    var point = new Models.Point
                    {
                        Type = PointType.Game,
                        IdCode = requestDto.IdCode,
                        EnglishName = requestDto.EnglishName,
                        HeaderImage = requestDto.HeaderImage,
                        AvatarImage = requestDto.AvatarImage
                    };

                    if (!string.IsNullOrWhiteSpace(requestDto.ChineseName))
                        point.ChineseName = requestDto.ChineseName;

                    _dbContext.Points.Add(point);
                    _dbContext.PointRelationships.AddRange((await _dbContext.Points
                        .Where(p => requestDto.PlatformPoitns.Contains(p.IdCode) && p.Type == PointType.Platform)
                        .Select(p => p.Id)
                        .ToListAsync())
                        .Select(id => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Platform,
                            SourcePointId = point.Id,
                            TargetPointId = id
                        }));
                    await _dbContext.SaveChangesAsync();
                    break;
                }

                case CreateOneRequestDto.CreateType.Hardware:
                {
                    if (string.IsNullOrWhiteSpace(requestDto.HeaderImage))
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.HeaderImage), Errors.Required);

                    var point = new Models.Point
                    {
                        Type = PointType.Hardware,
                        IdCode = requestDto.IdCode,
                        EnglishName = requestDto.EnglishName,
                        HeaderImage = requestDto.HeaderImage
                    };

                    if (!string.IsNullOrWhiteSpace(requestDto.ChineseName))
                        point.ChineseName = requestDto.ChineseName;

                    _dbContext.Points.Add(point);
                    await _dbContext.SaveChangesAsync();
                    break;
                }

                case CreateOneRequestDto.CreateType.Vendor:
                {
                    var point = new Models.Point
                    {
                        Type = PointType.Vendor,
                        IdCode = requestDto.IdCode,
                        EnglishName = requestDto.EnglishName
                    };

                    if (!string.IsNullOrWhiteSpace(requestDto.ChineseName))
                        point.ChineseName = requestDto.ChineseName;
                    if (!string.IsNullOrWhiteSpace(requestDto.AvatarImage))
                        point.AvatarImage = requestDto.AvatarImage;

                    _dbContext.Points.Add(point);
                    await _dbContext.SaveChangesAsync();
                    break;
                }

                case CreateOneRequestDto.CreateType.Category:
                {
                    var point = new Models.Point
                    {
                        Type = PointType.Category,
                        IdCode = requestDto.IdCode,
                        EnglishName = requestDto.EnglishName
                    };

                    if (!string.IsNullOrWhiteSpace(requestDto.ChineseName))
                        point.ChineseName = requestDto.ChineseName;
                    if (!string.IsNullOrWhiteSpace(requestDto.AvatarImage))
                        point.AvatarImage = requestDto.AvatarImage;

                    _dbContext.Points.Add(point);
                    await _dbContext.SaveChangesAsync();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Ok();
        }

        /// <summary>
        /// 特性据点关联黑名单
        /// </summary>
        private static List<string> _tagBlacklist = new List<string>
        {
            "Action",
            "Indie",
            "Adventure",
            "RPG",
            "Multiplayer",
            "Open World",
            "Casual",
            "Singleplayer",
            "Early Access",
            "Co-op",
            "Free to Play",
            "Story Rich",
            "Classic",
            "Local Co-Op"
        };

        /// <summary>
        /// CreateOne request DTO
        /// </summary>
        public class CreateOneRequestDto
        {
            /// <summary>
            /// 开设据点类型
            /// </summary>
            public CreateType Type { get; set; }

            /// <summary>
            /// Steam App ID
            /// </summary>
            public int? SteamAppId { get; set; }

            /// <summary>
            /// 英文名
            /// </summary>
            [Required]
            public string EnglishName { get; set; }

            /// <summary>
            /// 中文名
            /// </summary>
            public string ChineseName { get; set; }

            /// <summary>
            /// 识别码
            /// </summary>
            [Required]
            public string IdCode { get; set; }

            /// <summary>
            /// 平台据点识别码列表
            /// </summary>
            public List<string> PlatformPoitns { get; set; }

            /// <summary>
            /// 头部图
            /// </summary>
            public string HeaderImage { get; set; }

            /// <summary>
            /// 头像
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            /// 开设据点类型
            /// </summary>
            public enum CreateType
            {
                /// <summary>
                /// Steam 游戏
                /// </summary>
                SteamGame,

                /// <summary>
                /// 其他游戏
                /// </summary>
                OtherGame,

                /// <summary>
                /// 硬件
                /// </summary>
                Hardware,

                /// <summary>
                /// 厂商
                /// </summary>
                Vendor,

                /// <summary>
                /// 类型
                /// </summary>
                Category
            }
        }
    }
}