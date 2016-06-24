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
using Keylol.Controllers.Point;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
//    [Authorize(Users = "Stackia")]
    // TODO: 迁移完成之后恢复权限验证
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolRoleManager _roleManager;
        private readonly KeylolUserManager _userManager;
        private readonly IModel _mqChannel;

        /// <summary>
        /// 创建 <see cref="DatabaseMigrationController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="roleManager"><see cref="KeylolRoleManager"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="mqChannel"><see cref="IModel"/></param>
        public DatabaseMigrationController(KeylolDbContext dbContext, KeylolRoleManager roleManager,
            KeylolUserManager userManager, IModel mqChannel)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _mqChannel = mqChannel;
        }

        /// <summary>
        /// 添加一个据点职员
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="staffId">职员 ID</param>
        /// <returns></returns>
        [Route("add-point-staff")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPointStaff(string pointId, string staffId)
        {
            _dbContext.PointStaff.Add(new PointStaff
            {
                PointId = pointId,
                StaffId = staffId
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        ///     创建角色，旧的 UserClaims 迁入新角色中
        /// </summary>
        [Route("migrate-1")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate1()
        {
            await _roleManager.CreateAsync(new IdentityRole(KeylolRoles.Operator));

            var operators = await _dbContext.Database
                .SqlQuery<string>("SELECT UserId FROM dbo.UserClaims WHERE ClaimType = 'staff'")
                .ToListAsync();

            var userManager = Global.Container.GetInstance<KeylolUserManager>();
            foreach (var @operator in operators)
            {
                await userManager.AddToRoleAsync(@operator, KeylolRoles.Operator);
            }
            return Ok("成功");
        }

        /// <summary>
        /// 解绑无效的机器人绑定
        /// </summary>
        [Route("migrate-2")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate2()
        {
            var usersToUnbindBot = await _dbContext.Database
                .SqlQuery<string>("SELECT UserId FROM dbo.UserClaims WHERE ClaimType = 'status'")
                .ToListAsync();
            foreach (var userId in usersToUnbindBot)
            {
                var user = await _userManager.FindByIdAsync(userId);
                user.SteamBotId = null;
            }
            await _dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM dbo.UserClaims");
            await _dbContext.SaveChangesAsync();
            return Ok("成功");
        }

        /// <summary>
        /// 创建新的平台据点
        /// </summary>
        [Route("migrate-3")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate3()
        {
            var pointsToCreate = new List<Models.Point>
            {
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "BTNET",
                    EnglishName = "Battle.net",
                    ChineseName = "战网"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "PLYSN",
                    EnglishName = "PlayStation"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "MSBOX",
                    EnglishName = "Xbox"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "APIOS",
                    EnglishName = "iOS"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "ANDRD",
                    EnglishName = "Android"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "MSUWP",
                    EnglishName = "Windows UWP",
                    ChineseName = "Windows 通用应用"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "GMBYA",
                    EnglishName = "GBA"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NTDDS",
                    EnglishName = "NDS"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NT3DS",
                    EnglishName = "3DS"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NDWII",
                    EnglishName = "Wii"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NWIIU",
                    EnglishName = "WiiU"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "SYPSP",
                    EnglishName = "PSP"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "CLENT",
                    EnglishName = "Independent Client",
                    ChineseName = "独立客户端"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "WEBBD",
                    EnglishName = "Browser-Based",
                    ChineseName = "网页载体"
                }
            };
            _dbContext.Points.AddRange(pointsToCreate);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 旧据点资料填充，每次调用填充 200 个据点
        /// </summary>
        [Route("migrate-4")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate4()
        {
            var points = await _dbContext.Points
                .Where(p => p.Type == PointType.Game && p.SteamAppId != null && p.MediaHeaderImage == string.Empty)
                .OrderBy(p => p.Sid)
                .Take(200)
                .ToListAsync();
            var failedPoints = new List<string>();
            foreach (var point in points.AsParallel())
            {
                try
                {
                    var cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Uri("http://store.steampowered.com/"), new Cookie("birthtime", "-473410799"));
                    cookieContainer.Add(new Uri("http://store.steampowered.com/"), new Cookie("mature_content", "1"));
                    var request = WebRequest.CreateHttp(
                        $"http://store.steampowered.com/app/{point.SteamAppId}/?l=english&cc=us");
                    var picsRequest = WebRequest.CreateHttp(
                        $"https://steampics-mckay.rhcloud.com/info?apps={point.SteamAppId}");
                    picsRequest.AutomaticDecompression = request.AutomaticDecompression
                        = DecompressionMethods.Deflate | DecompressionMethods.GZip;
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
                            throw new Exception();

                        if (dom[".game_area_dlc_bubble"].Any())
                            throw new Exception();

                        foreach (var spec in dom[".game_area_details_specs a.name"])
                        {
                            switch (spec.InnerText)
                            {
                                case "Multi-player":
                                    point.MultiPlayer = true;
                                    break;

                                case "Single-player":
                                    point.SinglePlayer = true;
                                    break;

                                case "Co-op":
                                    point.Coop = true;
                                    break;

                                case "Captions available":
                                    point.CaptionsAvailable = true;
                                    break;

                                case "Commentary available":
                                    point.CommentaryAvailable = true;
                                    break;

                                case "Includes level editor":
                                    point.IncludeLevelEditor = true;
                                    break;

                                case "Steam Achievements":
                                    point.Achievements = true;
                                    break;

                                case "Steam Cloud":
                                    point.Cloud = true;
                                    break;

                                case "Local Co-op":
                                    point.LocalCoop = true;
                                    break;

                                case "Steam Trading Cards":
                                    point.SteamTradingCards = true;
                                    break;

                                case "Steam Workshop":
                                    point.SteamWorkshop = true;
                                    break;

                                case "In-App Purchases":
                                    point.InAppPurchases = true;
                                    break;
                            }
                        }

                        var chineseAvailability = new ChineseAvailability
                        {
                            ThirdPartyLinks = new List<ChineseAvailability.ThirdPartyLink>()
                        };
                        foreach (var tr in dom[".game_language_options tr"])
                        {
                            var tds = tr.ChildElements.ToList();
                            if (tds.Count < 4)
                                continue;
                            var language = new ChineseAvailability.Language
                            {
                                Interface = tds[1].ChildElements.Any(),
                                FullAudio = tds[2].ChildElements.Any(),
                                Subtitles = tds[3].ChildElements.Any()
                            };
                            switch (tds[0].InnerText.Trim())
                            {
                                case "English":
                                    chineseAvailability.English = language;
                                    break;

                                case "Japanese":
                                    chineseAvailability.Japanese = language;
                                    break;

                                case "Simplified Chinese":
                                    chineseAvailability.SimplifiedChinese = language;
                                    break;

                                case "Traditional Chinese":
                                    chineseAvailability.TraditionalChinese = language;
                                    break;
                            }
                        }
                        point.ChineseAvailability = JsonConvert.SerializeObject(chineseAvailability,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

                        if (string.IsNullOrWhiteSpace(point.TitleCoverImage))
                            point.TitleCoverImage = $"keylol://steam/app-headers/{point.SteamAppId}";

                        var screenshots = dom[".highlight_strip_screenshot img"];
                        var media = new List<PointMedia>(screenshots.Length);
                        point.HeaderImage = string.Empty;
                        point.MediaHeaderImage = string.Empty;
                        foreach (var screenshot in screenshots)
                        {
                            string link;
                            var match = Regex.Match(screenshot.Attributes["src"], @"ss_([^\/]*)\.\d+x\d+\.jpg");
                            if (match.Success)
                            {
                                link = $"keylol://steam/app-screenshots/{point.SteamAppId}-{match.Groups[1].Value}";
                            }
                            else
                            {
                                match = Regex.Match(screenshot.Attributes["src"], $@"{point.SteamAppId}\/([^\/]*?)\.");
                                if (match.Success)
                                    link =
                                        $"keylol://steam/app-resources/{point.SteamAppId}/{match.Groups[1].Value}.jpg";
                                else
                                    continue;
                            }
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
                        using (var picsResponse = await picsAwaiter)
                        {
                            var picsRs = picsResponse.GetResponseStream();
                            if (picsRs == null)
                                throw new Exception();
                            var sr = new StreamReader(picsRs);
                            var picsRoot = JToken.Parse(await sr.ReadToEndAsync());
                            if (!(bool) picsRoot["success"])
                                throw new Exception();
                            var thumbnailImageHash =
                                (string) picsRoot["apps"][point.SteamAppId.ToString()]["common"]["logo"];
                            if (!string.IsNullOrWhiteSpace(thumbnailImageHash))
                                point.ThumbnailImage =
                                    $"keylol://steam/app-thumbnails/{point.SteamAppId}-{thumbnailImageHash}";
                            var avatarImagePrefix = $"keylol://steam/app-icons/{point.SteamAppId}-";
                            var avatarImageHash =
                                (string) picsRoot["apps"][point.SteamAppId.ToString()]["common"]["icon"];
                            if (string.IsNullOrWhiteSpace(point.AvatarImage) ||
                                point.AvatarImage.StartsWith(avatarImagePrefix))
                            {
                                point.AvatarImage = string.IsNullOrWhiteSpace(avatarImageHash)
                                    ? string.Empty
                                    : $"{avatarImagePrefix}{avatarImageHash}";
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    failedPoints.Add($"{point.Id} {point.EnglishName}");
                }
            }
            if (failedPoints.Count > 0)
                return Ok(failedPoints);
            return Ok("成功");
        }

        /// <summary>
        /// 垃圾据点关系清理
        /// </summary>
        [Route("migrate-5")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate5()
        {
            var tagBlacklist = PointController.TagBlacklist;
            var relationships = await (from relationship in _dbContext.PointRelationships
                where relationship.Relationship == PointRelationshipType.Tag &&
                      tagBlacklist.Contains(relationship.TargetPoint.EnglishName)
                select relationship).ToListAsync();
            _dbContext.PointRelationships.RemoveRange(relationships);
            await _dbContext.SaveChangesAsync();
            return Ok("成功");
        }

        /// <summary>
        /// 文章封面图填充，内容图片重缩放，每次调用填充 200 篇文章
        /// </summary>
        [Route("migrate-6")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate6()
        {
            var articles = await _dbContext.Articles.Include(a => a.TargetPoint)
                .Where(a => a.CoverImage == string.Empty).Take(200).ToListAsync();
            Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
            foreach (var article in articles)
            {
                article.CoverImage = article.TargetPoint.HeaderImage;
                if (string.IsNullOrWhiteSpace(article.CoverImage))
                {
                    switch (article.TargetPoint.Type)
                    {
                        case PointType.Game:
                        case PointType.Hardware:
                            article.CoverImage = "keylol://05fe8398a92920133a6d19be859a84b5.jpg";
                            break;

                        case PointType.Category:
                            article.CoverImage = "keylol://1253e8594b3629de4a1a8053233848fb.jpg";
                            break;

                        case PointType.Vendor:
                            article.CoverImage = "keylol://44d25f29481e7156db37431a3942a418.jpg";
                            break;

                        case PointType.Platform:
                            article.CoverImage = "keylol://beec8dd58f207daccf88143e3df90372.jpg";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                var dom = CQ.Create(article.Content);
                foreach (var img in dom["img"])
                {
                    string oldWidthText;
                    if (!img.TryGetAttribute("width", out oldWidthText)) continue;
                    var oldWidth = int.Parse(oldWidthText);
                    if (oldWidth <= 670) continue;
                    img.SetAttribute("width", "670");
                    string oldheightText;
                    if (!img.TryGetAttribute("height", out oldheightText)) continue;
                    var oldHeight = int.Parse(oldheightText);
                    img.SetAttribute("height", ((int) ((double) oldHeight*670/oldWidth)).ToString());
                }
                article.Content = dom.Render();
            }
            await _dbContext.SaveChangesAsync();
            return Ok("成功");
        }

        /// <summary>
        /// 旧文章评论富文本化，回复关系关联，UnstyledContent 填充
        /// </summary>
        [Route("migrate-7")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate7()
        {
            var comments =
                await _dbContext.ArticleComments.Where(c => c.UnstyledContent == string.Empty).ToListAsync();
            foreach (var comment in comments)
            {
                var matches = Regex.Matches(comment.Content, "^(?:#(\\d+)[ \\t]*)+(?:$|[ \\t]+)", RegexOptions.Multiline);
                if (matches.Count > 0)
                {
                    var sidForArticle =
                        (from Match match in matches
                            from Capture capture in match.Groups[1].Captures
                            select int.Parse(capture.Value)).First();
                    var replyToCommentId = await _dbContext.ArticleComments.Where(
                        c => c.ArticleId == comment.ArticleId && c.SidForArticle == sidForArticle)
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync();
                    if (replyToCommentId != null)
                        comment.ReplyToCommentId = replyToCommentId;
                }
                comment.Content = $"<p>{Regex.Replace(comment.Content, @"\r\n?|\n", "<br>")}</p>";
                comment.UnstyledContent = PlainTextFormatter.FlattenHtml(comment.Content, false);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 所有文章重新推送
        /// </summary>
        [Route("migrate-8")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate8()
        {
            var articleIds = await _dbContext.Articles.OrderBy(a => a.Sid).Select(a => a.Id).ToListAsync();
            foreach (var articleId in articleIds)
            {
                _mqChannel.SendMessage(string.Empty, MqClientProvider.PushHubRequestQueue, new PushHubRequestDto
                {
                    Type = ContentPushType.Article,
                    ContentId = articleId
                });
            }
            return Ok("成功");
        }
    }
}