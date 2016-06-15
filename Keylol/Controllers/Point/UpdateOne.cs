using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Point
{
    public partial class PointController
    {
        /// <summary>
        /// 更新指定据点的属性
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="requestDto">请求 DTO</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> UpdateOne(string id, [NotNull] UpdateOneRequestDto requestDto)
        {
            var point = await _dbContext.Points.FindAsync(id);
            if (point == null)
                return NotFound();

            if (requestDto.ChineseName != null)
                point.ChineseName = requestDto.ChineseName;

            if (!string.IsNullOrWhiteSpace(requestDto.EnglishName))
                point.EnglishName = requestDto.EnglishName;

            if (requestDto.ChineseAliases != null)
                point.ChineseAliases = requestDto.ChineseAliases;

            if (requestDto.EnglishAliases != null)
                point.EnglishAliases = requestDto.EnglishAliases;

            if (requestDto.HeaderImage != null && Helpers.IsTrustedUrl(requestDto.HeaderImage))
                point.HeaderImage = requestDto.HeaderImage;

            if (requestDto.AvatarImage != null && Helpers.IsTrustedUrl(requestDto.AvatarImage))
                point.AvatarImage = requestDto.AvatarImage;

            if (requestDto.Logo != null && Helpers.IsTrustedUrl(requestDto.Logo))
                point.Logo = requestDto.Logo;

            if (requestDto.ThemeColor != null)
            {
                try
                {
                    point.ThemeColor = ColorTranslator.ToHtml(ColorTranslator.FromHtml(requestDto.ThemeColor));
                }
                catch (Exception)
                {
                    point.ThemeColor = string.Empty;
                }
            }

            if (requestDto.LightThemeColor != null)
            {
                try
                {
                    point.LightThemeColor = ColorTranslator.ToHtml(ColorTranslator.FromHtml(requestDto.LightThemeColor));
                }
                catch (Exception)
                {
                    point.LightThemeColor = string.Empty;
                }
            }

            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                #region 更新商店信息

                if (requestDto.SteamAppId != null && requestDto.SteamAppId != point.SteamAppId)
                {
                    if (requestDto.SteamAppId <= 0)
                    {
                        point.SteamAppId = null;
                    }
                    else
                    {
                        if (await _dbContext.Points.AnyAsync(p => p.SteamAppId == requestDto.SteamAppId))
                            return this.BadRequest(nameof(requestDto), nameof(requestDto.SteamAppId), Errors.Duplicate);
                        point.SteamAppId = requestDto.SteamAppId;
                    }
                }

                if (requestDto.SonkwoProductId != null && requestDto.SonkwoProductId != point.SonkwoProductId)
                {
                    if (requestDto.SonkwoProductId <= 0)
                    {
                        point.SonkwoProductId = null;
                    }
                    else
                    {
                        if (await _dbContext.Points.AnyAsync(p => p.SonkwoProductId == requestDto.SonkwoProductId))
                            return this.BadRequest(nameof(requestDto), nameof(requestDto.SonkwoProductId),
                                Errors.Duplicate);
                        point.SonkwoProductId = requestDto.SonkwoProductId;
                    }
                }

                if (requestDto.UplayLink != null)
                    point.UplayLink = requestDto.UplayLink;

                if (requestDto.UplayPrice != null)
                    point.UplayPrice = requestDto.UplayPrice;

                if (requestDto.XboxLink != null)
                    point.XboxLink = requestDto.XboxLink;

                if (requestDto.XboxPrice != null)
                    point.XboxPrice = requestDto.XboxPrice;

                if (requestDto.PlayStationLink != null)
                    point.PlayStationLink = requestDto.PlayStationLink;

                if (requestDto.PlayStationPrice != null)
                    point.PlayStationPrice = requestDto.PlayStationPrice;

                if (requestDto.OriginLink != null)
                    point.OriginLink = requestDto.OriginLink;

                if (requestDto.OriginPrice != null)
                    point.OriginPrice = requestDto.OriginPrice;

                if (requestDto.WindowsStoreLink != null)
                    point.WindowsStoreLink = requestDto.WindowsStoreLink;

                if (requestDto.WindowsStorePrice != null)
                    point.WindowsStorePrice = requestDto.WindowsStorePrice;

                if (requestDto.AppStoreLink != null)
                    point.AppStoreLink = requestDto.AppStoreLink;

                if (requestDto.AppStorePrice != null)
                    point.AppStorePrice = requestDto.AppStorePrice;

                if (requestDto.GooglePlayLink != null)
                    point.GooglePlayLink = requestDto.GooglePlayLink;

                if (requestDto.GooglePlayPrice != null)
                    point.GooglePlayPrice = requestDto.GooglePlayPrice;

                if (requestDto.GogLink != null)
                    point.GogLink = requestDto.GogLink;

                if (requestDto.GogPrice != null)
                    point.GogPrice = requestDto.GogPrice;

                if (requestDto.BattleNetLink != null)
                    point.BattleNetLink = requestDto.BattleNetLink;

                if (requestDto.BattleNetPrice != null)
                    point.BattleNetPrice = requestDto.BattleNetPrice;

                #endregion

                if (requestDto.GenrePoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Genre)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.GenrePoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Genre,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                if (requestDto.TagPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Tag)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.TagPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Tag,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                if (requestDto.MediaHeaderImage != null && Helpers.IsTrustedUrl(requestDto.MediaHeaderImage))
                    point.MediaHeaderImage = requestDto.MediaHeaderImage;

                if (requestDto.TitleCoverImage != null && Helpers.IsTrustedUrl(requestDto.TitleCoverImage))
                    point.TitleCoverImage = requestDto.TitleCoverImage;

                if (requestDto.ThumbnailImage != null && Helpers.IsTrustedUrl(requestDto.ThumbnailImage))
                    point.ThumbnailImage = requestDto.ThumbnailImage;
            }

            if (point.Type == PointType.Game)
            {
                if (requestDto.PlatformPoints != null)
                {
                    var platforms = (await _dbContext.Points
                        .Where(p => requestDto.PlatformPoints.Contains(p.IdCode) && p.Type == PointType.Platform)
                        .Select(p => p.Id)
                        .ToListAsync())
                        .Select(platformPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Platform,
                            SourcePointId = point.Id,
                            TargetPointId = platformPointId
                        }).ToList();
                    if (platforms.Count > 0)
                    {
                        _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                            .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Platform)
                            .ToListAsync());
                        _dbContext.PointRelationships.AddRange(platforms);
                    }
                }

                #region 更新特性属性

                if (requestDto.MultiPlayer != null)
                    point.MultiPlayer = requestDto.MultiPlayer.Value;

                if (requestDto.SinglePlayer != null)
                    point.SinglePlayer = requestDto.SinglePlayer.Value;

                if (requestDto.Coop != null)
                    point.Coop = requestDto.Coop.Value;

                if (requestDto.CaptionsAvailable != null)
                    point.CaptionsAvailable = requestDto.CaptionsAvailable.Value;

                if (requestDto.CommentaryAvailable != null)
                    point.CommentaryAvailable = requestDto.CommentaryAvailable.Value;

                if (requestDto.IncludeLevelEditor != null)
                    point.IncludeLevelEditor = requestDto.IncludeLevelEditor.Value;

                if (requestDto.Achievements != null)
                    point.Achievements = requestDto.Achievements.Value;

                if (requestDto.Cloud != null)
                    point.Cloud = requestDto.Cloud.Value;

                if (requestDto.LocalCoop != null)
                    point.LocalCoop = requestDto.LocalCoop.Value;

                if (requestDto.SteamTradingCards != null)
                    point.SteamTradingCards = requestDto.SteamTradingCards.Value;

                if (requestDto.SteamWorkshop != null)
                    point.SteamWorkshop = requestDto.SteamWorkshop.Value;

                if (requestDto.InAppPurchases != null)
                    point.InAppPurchases = requestDto.InAppPurchases.Value;

                #endregion

                if (requestDto.DeveloperPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Developer)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.DeveloperPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Developer,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                if (requestDto.PublisherPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Publisher)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.PublisherPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Publisher,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                if (requestDto.ResellerPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Reseller)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.ResellerPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Reseller,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                if (requestDto.SeriesPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Series)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.SeriesPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Series,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }

                // 用 1989 年 6 月 4 日作为一个特殊值，触发删除操作

                if (requestDto.PublishDate != null)
                    point.PublishDate = requestDto.PublishDate.Value.Date == new DateTime(1989, 6, 4)
                        ? (DateTime?) null
                        : requestDto.PublishDate.Value.Date;

                if (requestDto.PreOrderDate != null)
                    point.PreOrderDate = requestDto.PreOrderDate.Value.Date == new DateTime(1989, 6, 4)
                        ? (DateTime?) null
                        : requestDto.PreOrderDate.Value.Date;

                if (requestDto.ReleaseDate != null)
                    point.ReleaseDate = requestDto.ReleaseDate.Value.Date == new DateTime(1989, 6, 4)
                        ? (DateTime?) null
                        : requestDto.ReleaseDate.Value.Date;

                var chineseAvailability = Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability) ??
                                          new ChineseAvailability
                                          {
                                              English = new ChineseAvailability.Language
                                              {
                                                  Interface = true,
                                                  Subtitles = true,
                                                  FullAudio = true
                                              },
                                              ThirdPartyLinks = new List<ChineseAvailability.ThirdPartyLink>()
                                          };

                if (requestDto.EnglishLanguage != null)
                    chineseAvailability.English = requestDto.EnglishLanguage;

                if (requestDto.JapaneseLanguage != null)
                    chineseAvailability.Japanese = requestDto.JapaneseLanguage;

                if (requestDto.SimplifiedChineseLanguage != null)
                    chineseAvailability.SimplifiedChinese = requestDto.SimplifiedChineseLanguage;

                if (requestDto.TraditionalChineseLanguage != null)
                    chineseAvailability.TraditionalChinese = requestDto.TraditionalChineseLanguage;

                if (requestDto.ChineseThirdPartyLinks != null)
                    chineseAvailability.ThirdPartyLinks = requestDto.ChineseThirdPartyLinks;

                point.ChineseAvailability = JsonConvert.SerializeObject(chineseAvailability,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            }
            else if (point.Type == PointType.Hardware)
            {
                if (requestDto.ManufacturerPoints != null)
                {
                    _dbContext.PointRelationships.RemoveRange(await _dbContext.PointRelationships
                        .Where(r => r.SourcePointId == point.Id && r.Relationship == PointRelationshipType.Manufacturer)
                        .ToListAsync());
                    _dbContext.PointRelationships.AddRange(requestDto.TagPoints
                        .Select(targetPointId => new PointRelationship
                        {
                            Relationship = PointRelationshipType.Manufacturer,
                            SourcePointId = point.Id,
                            TargetPointId = targetPointId
                        }));
                }
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// UpdateOne request DTO
        /// </summary>
        public class UpdateOneRequestDto
        {
            /// <summary>
            /// 中文名
            /// </summary>
            [MaxLength(60)]
            public string ChineseName { get; set; }

            /// <summary>
            /// 英文名
            /// </summary>
            [MaxLength(100)]
            public string EnglishName { get; set; }

            /// <summary>
            /// 中文索引
            /// </summary>
            [MaxLength(128)]
            public string ChineseAliases { get; set; }

            /// <summary>
            /// 英文索引
            /// </summary>
            [MaxLength(128)]
            public string EnglishAliases { get; set; }

            /// <summary>
            /// 平台据点识别码列表
            /// </summary>
            public List<string> PlatformPoints { get; set; }

            #region 商店信息

            /// <summary>
            /// Steam App ID
            /// </summary>
            [Range(0, int.MaxValue)]
            public int? SteamAppId { get; set; }

            /// <summary>
            /// 杉果 Product ID
            /// </summary>
            [Range(0, int.MaxValue)]
            public int? SonkwoProductId { get; set; }

            /// <summary>
            /// Uplay 链接
            /// </summary>
            [MaxLength(512)]
            public string UplayLink { get; set; }

            /// <summary>
            /// Uplay 价格
            /// </summary>
            [MaxLength(25)]
            public string UplayPrice { get; set; }

            /// <summary>
            /// Xbox 链接
            /// </summary>
            [MaxLength(512)]
            public string XboxLink { get; set; }

            /// <summary>
            /// Xbox 价格
            /// </summary>
            [MaxLength(25)]
            public string XboxPrice { get; set; }

            /// <summary>
            /// PlayStation 链接
            /// </summary>
            [MaxLength(512)]
            public string PlayStationLink { get; set; }

            /// <summary>
            /// PlayStation 价格
            /// </summary>
            [MaxLength(25)]
            public string PlayStationPrice { get; set; }

            /// <summary>
            /// Origin 链接
            /// </summary>
            [MaxLength(512)]
            public string OriginLink { get; set; }

            /// <summary>
            /// Origin 价格
            /// </summary>
            [MaxLength(25)]
            public string OriginPrice { get; set; }

            /// <summary>
            /// Windows Store 链接
            /// </summary>
            [MaxLength(512)]
            public string WindowsStoreLink { get; set; }

            /// <summary>
            /// Windows Store 价格
            /// </summary>
            [MaxLength(25)]
            public string WindowsStorePrice { get; set; }

            /// <summary>
            /// App Store 链接
            /// </summary>
            [MaxLength(512)]
            public string AppStoreLink { get; set; }

            /// <summary>
            /// App Store 价格
            /// </summary>
            [MaxLength(25)]
            public string AppStorePrice { get; set; }

            /// <summary>
            /// Google Play 链接
            /// </summary>
            [MaxLength(512)]
            public string GooglePlayLink { get; set; }

            /// <summary>
            /// Google Play 价格
            /// </summary>
            [MaxLength(25)]
            public string GooglePlayPrice { get; set; }

            /// <summary>
            /// Gog 链接
            /// </summary>
            [MaxLength(512)]
            public string GogLink { get; set; }

            /// <summary>
            /// GOG 价格
            /// </summary>
            [MaxLength(25)]
            public string GogPrice { get; set; }

            /// <summary>
            /// 战网链接
            /// </summary>
            [MaxLength(512)]
            public string BattleNetLink { get; set; }

            /// <summary>
            /// 战网价格
            /// </summary>
            [MaxLength(25)]
            public string BattleNetPrice { get; set; }

            #endregion

            #region 特性属性

            /// <summary>
            /// 多人游戏
            /// </summary>
            public bool? MultiPlayer { get; set; }

            /// <summary>
            /// 单人游戏
            /// </summary>
            public bool? SinglePlayer { get; set; }

            /// <summary>
            /// 合作
            /// </summary>
            public bool? Coop { get; set; }

            /// <summary>
            /// 视听字幕
            /// </summary>
            public bool? CaptionsAvailable { get; set; }

            /// <summary>
            /// 旁白解说
            /// </summary>
            public bool? CommentaryAvailable { get; set; }

            /// <summary>
            /// 关卡客制化
            /// </summary>
            public bool? IncludeLevelEditor { get; set; }

            /// <summary>
            /// 成就系统
            /// </summary>
            public bool? Achievements { get; set; }

            /// <summary>
            /// 云存档
            /// </summary>
            public bool? Cloud { get; set; }

            /// <summary>
            /// 本地多人
            /// </summary>
            public bool? LocalCoop { get; set; }

            /// <summary>
            /// Steam 卡牌
            /// </summary>
            public bool? SteamTradingCards { get; set; }

            /// <summary>
            /// Steam 创意工坊
            /// </summary>
            public bool? SteamWorkshop { get; set; }

            /// <summary>
            /// 内购
            /// </summary>
            public bool? InAppPurchases { get; set; }

            #endregion

            /// <summary>
            /// 开发厂据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> DeveloperPoints { get; set; }

            /// <summary>
            /// 发行商据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> PublisherPoints { get; set; }

            /// <summary>
            /// 代理商据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> ResellerPoints { get; set; }

            /// <summary>
            /// 流派据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> GenrePoints { get; set; }

            /// <summary>
            /// 特性据点 ID 列表
            /// </summary>
            [MaxLength(15)]
            public List<string> TagPoints { get; set; }

            /// <summary>
            /// 系列据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> SeriesPoints { get; set; }

            /// <summary>
            /// 制造厂据点 ID 列表
            /// </summary>
            [MaxLength(6)]
            public List<string> ManufacturerPoints { get; set; }

            /// <summary>
            /// 公开日期
            /// </summary>
            public DateTime? PublishDate { get; set; }

            /// <summary>
            /// 预购日期
            /// </summary>
            public DateTime? PreOrderDate { get; set; }

            /// <summary>
            /// 发行日期
            /// </summary>
            public DateTime? ReleaseDate { get; set; }

            /// <summary>
            /// 英语可用度
            /// </summary>
            public ChineseAvailability.Language EnglishLanguage { get; set; }

            /// <summary>
            /// 日语可用度
            /// </summary>
            public ChineseAvailability.Language JapaneseLanguage { get; set; }

            /// <summary>
            /// 简体中文可用度
            /// </summary>
            public ChineseAvailability.Language SimplifiedChineseLanguage { get; set; }

            /// <summary>
            /// 繁体中文可用度
            /// </summary>
            public ChineseAvailability.Language TraditionalChineseLanguage { get; set; }

            /// <summary>
            /// 第三方汉化链接列表
            /// </summary>
            public List<ChineseAvailability.ThirdPartyLink> ChineseThirdPartyLinks { get; set; }

            /// <summary>
            /// 页眉图片
            /// </summary>
            public string HeaderImage { get; set; }

            /// <summary>
            /// 头像
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            /// 媒体中心封面
            /// </summary>
            public string MediaHeaderImage { get; set; }

            /// <summary>
            /// 标题封面
            /// </summary>
            public string TitleCoverImage { get; set; }

            /// <summary>
            /// 缩略图
            /// </summary>
            public string ThumbnailImage { get; set; }

            /// <summary>
            /// Logo
            /// </summary>
            public string Logo { get; set; }

            /// <summary>
            /// 主题色
            /// </summary>
            public string ThemeColor { get; set; }

            /// <summary>
            /// 轻主题色
            /// </summary>
            public string LightThemeColor { get; set; }
        }
    }
}