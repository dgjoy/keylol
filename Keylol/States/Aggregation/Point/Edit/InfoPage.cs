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
                infoPage.GenrePoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Genre
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList();

                infoPage.TagPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Tag
                    select new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList();
            }

            if (point.Type == PointType.Game)
            {
                infoPage.PlatformPoints = await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Platform
                    select relationship.TargetPoint.IdCode)
                    .ToListAsync();

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
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
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
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
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
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
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
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
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
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
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
        public List<string> PlatformPoints { get; set; }

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