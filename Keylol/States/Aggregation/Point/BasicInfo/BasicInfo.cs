using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.Point.BasicInfo
{
    /// <summary>
    /// 据点基础信息
    /// </summary>
    public class BasicInfo
    {
        /// <summary>
        /// 创建 <see cref="BasicInfo"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="point">据点对象</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="BasicInfo"/></returns>
        public static async Task<BasicInfo> CreateAsync(string currentUserId, Models.Point point,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            if (point.Type == PointType.Game)
                SteamCrawlerProvider.UpdatePointPrice(point.Id);
            var rating = await cachedData.Points.GetRatingsAsync(point.Id);
            return new BasicInfo
            {
                Id = point.Id,
                HeaderImage = point.HeaderImage,
                AvatarImage = point.AvatarImage,
                ChineseName = point.ChineseName,
                EnglishName = point.EnglishName,
                Categories = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Tag
                    select new
                    {
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        IdCode = p.IdCode,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList(),
                Vendors = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Developer ||
                          relationship.Relationship == PointRelationshipType.Manufacturer
                    select new
                    {
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        IdCode = p.IdCode,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList(),
                TitleCoverImage = point.TitleCoverImage,
                TotalPlayedTime = (await dbContext.UserSteamGameRecords
                    .Where(r => r.UserId == currentUserId && r.SteamAppId == point.SteamAppId)
                    .SingleOrDefaultAsync())?.TotalPlayedTime,
                KeylolAveragePlayedTime = Math.Round(await dbContext.UserSteamGameRecords
                    .Where(r => r.SteamAppId == point.SteamAppId)
                    .Select(r => r.TotalPlayedTime)
                    .DefaultIfEmpty()
                    .AverageAsync(), 1),
                OneStarCount = rating.OneStarCount,
                TwoStarCount = rating.TwoStarCount,
                ThreeStarCount = rating.ThreeStarCount,
                FourStarCount = rating.FourStarCount,
                FiveStarCount = rating.FiveStarCount,
                AverageRating = rating.AverageRating,
                SteamAppId = point.SteamAppId,
                SteamPrice = point.SteamPrice,
                SteamDiscountedPrice = point.SteamDiscountedPrice,
                SonkwoProductId = point.SonkwoProductId,
                SonkwoPrice = point.SonkwoPrice,
                SonkwoDiscountedPrice = point.SonkwoDiscountedPrice,
                UplayLink = point.UplayLink,
                UplayPrice = point.UplayPrice,
                XboxLink = point.XboxLink,
                XboxPrice = point.XboxPrice,
                PlayStationLink = point.PlayStationLink,
                PlayStationPrice = point.PlayStationPrice,
                OriginLink = point.OriginLink,
                OriginPrice = point.OriginPrice,
                WindowsStoreLink = point.WindowsStoreLink,
                WindowsStorePrice = point.WindowsStorePrice,
                AppStoreLink = point.AppStoreLink,
                AppStorePrice = point.AppStorePrice,
                GooglePlayLink = point.GooglePlayLink,
                GooglePlayPrice = point.GooglePlayPrice,
                GogLink = point.GogLink,
                GogPrice = point.GogPrice,
                BattleNetLink = point.BattleNetLink,
                BattleNetPrice = point.BattleNetPrice,
                Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, point.Id,
                        SubscriptionTargetType.Point),
                InLibrary = string.IsNullOrWhiteSpace(currentUserId) || point.SteamAppId == null
                    ? (bool?) null
                    : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, point.SteamAppId.Value)
            };
        }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 头部图
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public List<SimplePoint> Categories { get; set; }

        /// <summary>
        /// 厂商
        /// </summary>
        public List<SimplePoint> Vendors { get; set; }

        /// <summary>
        /// 标题封面
        /// </summary>
        public string TitleCoverImage { get; set; }

        /// <summary>
        /// 当前用户在档时间
        /// </summary>
        public double? TotalPlayedTime { get; set; }

        /// <summary>
        /// 其乐用户平均在档时间
        /// </summary>
        public double? KeylolAveragePlayedTime { get; set; }

        /// <summary>
        /// 一星评分个数
        /// </summary>
        public int OneStarCount { get; set; }

        /// <summary>
        /// 二星评分个数
        /// </summary>
        public int TwoStarCount { get; set; }

        /// <summary>
        /// 三星评分个数
        /// </summary>
        public int ThreeStarCount { get; set; }

        /// <summary>
        /// 四星评分个数
        /// </summary>
        public int FourStarCount { get; set; }

        /// <summary>
        /// 五星评分个数
        /// </summary>
        public int FiveStarCount { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

        #region 商店信息

        /// <summary>
        /// Steam App ID
        /// </summary>
        public int? SteamAppId { get; set; }

        /// <summary>
        /// Steam 价格
        /// </summary>
        public double? SteamPrice { get; set; }

        /// <summary>
        /// Steam 折后价格
        /// </summary>
        public double? SteamDiscountedPrice { get; set; }

        /// <summary>
        /// 杉果 Product ID
        /// </summary>
        public int? SonkwoProductId { get; set; }

        /// <summary>
        /// 杉果价格
        /// </summary>
        public double? SonkwoPrice { get; set; }

        /// <summary>
        /// 杉果折后价格
        /// </summary>
        public double? SonkwoDiscountedPrice { get; set; }

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

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}