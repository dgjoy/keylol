using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.ServiceBase;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point.Edit
{
    /// <summary>
    /// 聚合 - 据点 - 编辑 - 信息
    /// </summary>
    public class InfoPage
    {
        /// <summary>
        /// 获取信息页
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="InfoPage"/></returns>
        public static async Task<InfoPage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new InfoPage();
            return await CreateAsync(point, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="InfoPage"/>
        /// </summary>
        /// <param name="point">据点对象</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="InfoPage"/></returns>
        public static async Task<InfoPage> CreateAsync(Models.Point point, KeylolDbContext dbContext)
        {
            var infoPage = new InfoPage
            {
                ChineseAliases = point.ChineseAliases,
                EnglishAliases = point.EnglishAliases
            };

            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                #region 商店信息

                infoPage.SteamAppId = point.SteamAppId;
                infoPage.SonkwoProductId = point.SonkwoProductId;
                infoPage.UplayLink = point.UplayLink;
                infoPage.UplayPrice = point.UplayPrice;
                infoPage.XboxLink = point.XboxLink;
                infoPage.XboxPrice = point.XboxPrice;
                infoPage.PlayStationLink = point.PlayStationLink;
                infoPage.PlayStationPrice = point.PlayStationPrice;
                infoPage.OriginLink = point.OriginLink;
                infoPage.OriginPrice = point.OriginPrice;
                infoPage.WindowsStoreLink = point.WindowsStoreLink;
                infoPage.WindowsStorePrice = point.WindowsStorePrice;
                infoPage.AppStoreLink = point.AppStoreLink;
                infoPage.AppStorePrice = point.AppStorePrice;
                infoPage.GooglePlayLink = point.GooglePlayLink;
                infoPage.GooglePlayPrice = point.GooglePlayPrice;
                infoPage.GogLink = point.GogLink;
                infoPage.GogPrice = point.GogPrice;
                infoPage.BattleNetLink = point.BattleNetLink;
                infoPage.BattleNetPrice = point.BattleNetPrice;

                #endregion

                infoPage.GenrePoints = (await (from relationship in dbContext.PointRelationships
                                               where relationship.SourcePointId == point.Id &&
                                                     relationship.Relationship == PointRelationshipType.Genre
                                               select new
                                               {
                                                   relationship.TargetPoint.Id,
                                                   relationship.TargetPoint.ChineseName,
                                                   relationship.TargetPoint.EnglishName
                                               })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList();

                infoPage.TagPoints = (await (from relationship in dbContext.PointRelationships
                                             where relationship.SourcePointId == point.Id &&
                                                   relationship.Relationship == PointRelationshipType.Tag
                                             select new
                                             {
                                                 relationship.TargetPoint.Id,
                                                 relationship.TargetPoint.ChineseName,
                                                 relationship.TargetPoint.EnglishName
                                             })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList();
            }

            if (point.Type == PointType.Game)
            {
                infoPage.PlatformPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Platform
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                #region 特性属性

                infoPage.MultiPlayer = point.MultiPlayer ? true : (bool?) null;
                infoPage.SinglePlayer = point.SinglePlayer ? true : (bool?) null;
                infoPage.Coop = point.Coop ? true : (bool?) null;
                infoPage.CaptionsAvailable = point.CaptionsAvailable ? true : (bool?) null;
                infoPage.CommentaryAvailable = point.CommentaryAvailable ? true : (bool?) null;
                infoPage.IncludeLevelEditor = point.IncludeLevelEditor ? true : (bool?) null;
                infoPage.Achievements = point.Achievements ? true : (bool?) null;
                infoPage.Cloud = point.Cloud ? true : (bool?) null;
                infoPage.LocalCoop = point.LocalCoop ? true : (bool?) null;
                infoPage.SteamTradingCards = point.SteamTradingCards ? true : (bool?) null;
                infoPage.SteamWorkshop = point.SteamWorkshop ? true : (bool?) null;
                infoPage.InAppPurchases = point.InAppPurchases ? true : (bool?) null;

                #endregion

                infoPage.DeveloperPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Developer
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                infoPage.PublisherPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Publisher
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                infoPage.ResellerPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Reseller
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                infoPage.SeriesPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Series
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                infoPage.PointCreateTime = point.CreateTime;
                infoPage.PublishDate = point.PublishDate;
                infoPage.PreOrderDate = point.PreOrderDate;
                infoPage.ReleaseDate = point.ReleaseDate;

                infoPage.ChineseAvailability = Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability);
            }

            if (point.Type == PointType.Hardware)
            {
                infoPage.ManufacturerPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Manufacturer
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();
            }

            return infoPage;
        }

        /// <summary>
        /// 中文索引
        /// </summary>
        public string ChineseAliases { get; set; }

        /// <summary>
        /// 英文索引
        /// </summary>
        public string EnglishAliases { get; set; }

        /// <summary>
        /// 平台据点
        /// </summary>
        public List<SimplePoint> PlatformPoints { get; set; }

        #region 商店信息

        /// <summary>
        /// Steam App ID
        /// </summary>
        public int? SteamAppId { get; set; }

        /// <summary>
        /// 杉果 Product ID
        /// </summary>
        public int? SonkwoProductId { get; set; }

        /// <summary>
        /// Uplay 链接
        /// </summary>
        public string UplayLink { get; set; }

        /// <summary>
        /// Uplay 价格
        /// </summary>
        public double? UplayPrice { get; set; }

        /// <summary>
        /// Xbox 链接
        /// </summary>
        public string XboxLink { get; set; }

        /// <summary>
        /// Xbox 价格
        /// </summary>
        public double? XboxPrice { get; set; }

        /// <summary>
        /// PlayStation 链接
        /// </summary>
        public string PlayStationLink { get; set; }

        /// <summary>
        /// PlayStation 价格
        /// </summary>
        public double? PlayStationPrice { get; set; }

        /// <summary>
        /// Origin 链接
        /// </summary>
        public string OriginLink { get; set; }

        /// <summary>
        /// Origin 价格
        /// </summary>
        public double? OriginPrice { get; set; }

        /// <summary>
        /// Windows Store 链接
        /// </summary>
        public string WindowsStoreLink { get; set; }

        /// <summary>
        /// Windows Store 价格
        /// </summary>
        public double? WindowsStorePrice { get; set; }

        /// <summary>
        /// App Store 链接
        /// </summary>
        public string AppStoreLink { get; set; }

        /// <summary>
        /// App Store 价格
        /// </summary>
        public double? AppStorePrice { get; set; }

        /// <summary>
        /// Google Play 链接
        /// </summary>
        public string GooglePlayLink { get; set; }

        /// <summary>
        /// Google Play 价格
        /// </summary>
        public double? GooglePlayPrice { get; set; }

        /// <summary>
        /// Gog 链接
        /// </summary>
        public string GogLink { get; set; }

        /// <summary>
        /// GOG 价格
        /// </summary>
        public double? GogPrice { get; set; }

        /// <summary>
        /// 战网链接
        /// </summary>
        public string BattleNetLink { get; set; }

        /// <summary>
        /// 战网价格
        /// </summary>
        public double? BattleNetPrice { get; set; }

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
        /// 开发厂据点列表
        /// </summary>
        public List<SimplePoint> DeveloperPoints { get; set; }

        /// <summary>
        /// 发行商据点列表
        /// </summary>
        public List<SimplePoint> PublisherPoints { get; set; }

        /// <summary>
        /// 代理商据点列表
        /// </summary>
        public List<SimplePoint> ResellerPoints { get; set; }

        /// <summary>
        /// 流派据点列表
        /// </summary>
        public List<SimplePoint> GenrePoints { get; set; }

        /// <summary>
        /// 特性据点列表
        /// </summary>
        public List<SimplePoint> TagPoints { get; set; }

        /// <summary>
        /// 系列据点列表
        /// </summary>
        public List<SimplePoint> SeriesPoints { get; set; }

        /// <summary>
        /// 制造厂据点列表
        /// </summary>
        public List<SimplePoint> ManufacturerPoints { get; set; }

        /// <summary>
        /// 据点创建时间
        /// </summary>
        public DateTime? PointCreateTime { get; set; }

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
        /// 华语可用度
        /// </summary>
        public ChineseAvailability ChineseAvailability { get; set; }
    }
}