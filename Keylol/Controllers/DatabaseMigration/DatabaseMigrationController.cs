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
                    ChineseName = "战网",
                    ThemeColor = "#019ED3",
                    LightThemeColor = "#01CEE9",
                    AvatarImage = "keylol://db9679abd9d6b6365fa01e571b6f43bd.png",
                    HeaderImage = "keylol://e95b1ee1d9493df1064a7746423f0061.png",
                    Logo = "keylol://3482cd4b646b643c784aaaef6092bb2e.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "PLYSN",
                    EnglishName = "PlayStation",
                    ThemeColor = "#164DB2",
                    LightThemeColor = "#4671CA",
                    AvatarImage = "keylol://10bd9b82e38ba5b186bf52293fc7e593.png",
                    HeaderImage = "keylol://4c896df6713995a17e9aef1690337421.png",
                    Logo = "keylol://9479272ac21e0256b1dc56f60ee7ac47.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "MSBOX",
                    EnglishName = "Xbox",
                    ThemeColor = "#107C10",
                    LightThemeColor = "#76BC31",
                    AvatarImage = "keylol://2a12ef3e280c9a81c8f8c70c0845881b.png",
                    HeaderImage = "keylol://49378769ecce9fbbe9548537acff2a00.png",
                    Logo = "keylol://f0fe148e3db9a1eb2be0aa1279941b7a.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "APIOS",
                    EnglishName = "iOS",
                    ThemeColor = "#262626",
                    LightThemeColor = "#9E9E9E",
                    AvatarImage = "keylol://bf3c8b8bf71fef89bc77bc80f7754b8f.png",
                    HeaderImage = "keylol://ece9a690b758e023a7d4f9765fd4ab37.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "ANDRD",
                    EnglishName = "Android",
                    ChineseName = "安卓",
                    ThemeColor = "#72A441",
                    LightThemeColor = "#6AB344",
                    AvatarImage = "keylol://8e350bbdf59ef6a2a012acc577d24f41.png",
                    HeaderImage = "keylol://27f7096fb3b112fb3d3b1f7477908889.png",
                    Logo = "keylol://000a00ef8181dd0fa045fb7945062138.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "MSUWP",
                    EnglishName = "Windows UWP",
                    ChineseName = "Windows 通用应用",
                    ThemeColor = "#004A8C",
                    LightThemeColor = "#0078D7",
                    AvatarImage = "keylol://d0eb0713755cae9687a8c2a1b2d97ac0.png",
                    HeaderImage = "keylol://a2c2d81989ea4cf7bf0bb88b4c44f536.jpg",
                    Logo = "keylol://9aca888b6614fb1f550c4f295c509f0d.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "GMBYA",
                    EnglishName = "GBA",
                    ThemeColor = "#682C9E",
                    LightThemeColor = "#C66BFF",
                    AvatarImage = "keylol://c6d539233246f730845e8fb61ed7241f.png",
                    HeaderImage = "keylol://7b6c31c104466cfb5d8dbbfcea301c9c.png",
                    Logo = "keylol://4fa981d82262d8f8c6a614f28246ca8a.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NTDDS",
                    EnglishName = "NDS",
                    ThemeColor = "#292929",
                    LightThemeColor = "#979799",
                    AvatarImage = "keylol://54430472e52966097b12f547941714fd.png",
                    HeaderImage = "keylol://def88a37b6e902101e7dbdec7ce185e7.jpg",
                    Logo = "keylol://ed98a984f87197fe6bcd2c1fedeb6916.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NT3DS",
                    EnglishName = "3DS",
                    ThemeColor = "#231916",
                    LightThemeColor = "#D0000F",
                    AvatarImage = "keylol://b0fca4bb8bd6407680f9eff77b745cce.png",
                    HeaderImage = "keylol://def88a37b6e902101e7dbdec7ce185e7.jpg",
                    Logo = "keylol://92818f5c5aa23f03235687a1fae7ab07.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NDWII",
                    EnglishName = "Wii",
                    ThemeColor = "#231916",
                    LightThemeColor = "#999999",
                    AvatarImage = "keylol://58d32678ffe16b9ba88c36d52a44fcbe.png",
                    HeaderImage = "keylol://49bc2ce68780c2042021118ec855ec39.jpg",
                    Logo = "keylol://c92782037317d8dab462fd20f1aab7d1.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "NWIIU",
                    EnglishName = "WiiU",
                    ThemeColor = "#0097CB",
                    LightThemeColor = "#8D8D8D",
                    AvatarImage = "keylol://dfdfdf25b2f343c5e96a30f4b12cd151.png",
                    HeaderImage = "keylol://49bc2ce68780c2042021118ec855ec39.jpg",
                    Logo = "keylol://bdcfeb2c40a19ad2cb7cb9c0f4297ebc.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "SYPSP",
                    EnglishName = "PSP",
                    ThemeColor = "#000",
                    LightThemeColor = "#636363",
                    AvatarImage = "keylol://cb6e6ac22426f95ce39ddf85394c20f2.png",
                    HeaderImage = "keylol://59d1af5c46b92e15a609f1b22783e71d.png",
                    Logo = "keylol://3bf8927587d36b247ebf2d018a59badd.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "CLENT",
                    EnglishName = "Independent Client",
                    ChineseName = "独立客户端",
                    AvatarImage = "keylol://672a23c6096429911440f66f2dfdb301.png"
                },
                new Models.Point
                {
                    Type = PointType.Platform,
                    IdCode = "WEBBD",
                    EnglishName = "Browser-Based",
                    ChineseName = "网页载体",
                    AvatarImage = "keylol://57cbae033e037f7b76fbd1a2d795a90e.png"
                }
            };
            var steamPoint = await _dbContext.Points.SingleAsync(p => p.IdCode == "STEAM");
            steamPoint.AvatarImage = "keylol://0db6b9b30b44b578ea2b9882ab676e74.png";
            steamPoint.HeaderImage = "keylol://8fb9c1064940b5bf64ae10293e7e4cd0.png";
            steamPoint.Logo = "keylol://7051b0229858b80bfbe98ca798ec80d5.png";
            var originPoint = await _dbContext.Points.SingleAsync(p => p.IdCode == "ORGIN");
            originPoint.AvatarImage = "keylol://89c8a71ebfecb58db7a289ae657a0128.png";
            originPoint.HeaderImage = "keylol://68a51de472b626b1629490df7c672b25.jpg";
            originPoint.Logo = "keylol://529c00e4ede639b5b4d7c9e5a3f8ee3a.png";
            var uplayPoint = await _dbContext.Points.SingleAsync(p => p.IdCode == "UPLAY");
            uplayPoint.AvatarImage = "keylol://f910950f90d8e24826247c1ff6ccc4b3.png";
            uplayPoint.HeaderImage = "keylol://12981a7b9caef22230df2ad0462af879.png";
            uplayPoint.Logo = "keylol://777d8aa0c8ad41b3d003d7da24594f40.png";
            var keylolPoint = await _dbContext.Points.SingleAsync(p => p.IdCode == "KYLOL");
            keylolPoint.AvatarImage = "keylol://56535eac2fcbe5d19fb758d2390c8f45.png";
            keylolPoint.HeaderImage = "keylol://85acaa056bf5db1cadc0bcc0a3204b93.png";
            _dbContext.Points.AddRange(pointsToCreate);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 垃圾据点关系清理
        /// </summary>
        [Route("migrate-4")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate4()
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
        /// 旧文章评论富文本化，回复关系关联，UnstyledContent 填充
        /// </summary>
        [Route("migrate-5")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate5()
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
        [Route("migrate-6")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate6()
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

        /// <summary>
        /// 旧据点资料填充，每次调用填充 200 个据点
        /// </summary>
        [Route("migrate-7")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate7()
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
        /// 文章封面图填充，内容图片重缩放，每次调用填充 200 篇文章
        /// </summary>
        [Route("migrate-8")]
        [HttpPost]
        public async Task<IHttpActionResult> Migrate8()
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
    }
}