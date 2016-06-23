using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Shared
{
    /// <summary>
    /// 据点基础信息
    /// </summary>
    public class PointBasicInfo
    {
        /// <summary>
        /// 创建 <see cref="PointBasicInfo"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="point">据点对象</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointBasicInfo"/></returns>
        public static async Task<PointBasicInfo> CreateAsync(string currentUserId, Point point,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var basicInfo = new PointBasicInfo
            {
                Id = point.Id,
                IdCode = point.IdCode,
                Logo = point.Logo,
                ThemeColor = point.ThemeColor,
                LightThemeColor = point.LightThemeColor,
                Type = point.Type,
                HeaderImage = point.HeaderImage,
                AvatarImage = point.AvatarImage,
                ChineseName = point.ChineseName,
                EnglishName = point.EnglishName,
                Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, point.Id,
                        SubscriptionTargetType.Point)
            };

            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                var rating = await cachedData.Points.GetRatingsAsync(point.Id);
                basicInfo.OneStarCount = rating.OneStarCount;
                basicInfo.TwoStarCount = rating.TwoStarCount;
                basicInfo.ThreeStarCount = rating.ThreeStarCount;
                basicInfo.FourStarCount = rating.FourStarCount;
                basicInfo.FiveStarCount = rating.FiveStarCount;
                basicInfo.AverageRating = rating.AverageRating;

                basicInfo.Categories = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Tag
                    select new
                    {
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new PointBasicInfo
                    {
                        IdCode = p.IdCode,
                        ChineseName = string.IsNullOrWhiteSpace(p.ChineseName) ? p.EnglishName : p.ChineseName
                    })
                    .ToList();

                basicInfo.Vendors = (await (from relationship in dbContext.PointRelationships
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
                    .Select(p => new PointBasicInfo
                    {
                        IdCode = p.IdCode,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                basicInfo.TitleCoverImage = point.TitleCoverImage;

                #region 商店信息

                basicInfo.SteamAppId = point.SteamAppId;
                basicInfo.SteamPrice = point.SteamPrice;
                basicInfo.SteamDiscountedPrice = point.SteamDiscountedPrice;
                basicInfo.SonkwoProductId = point.SonkwoProductId;
                basicInfo.SonkwoPrice = point.SonkwoPrice;
                basicInfo.SonkwoDiscountedPrice = point.SonkwoDiscountedPrice;
                basicInfo.UplayLink = point.UplayLink;
                basicInfo.UplayPrice = point.UplayPrice;
                basicInfo.XboxLink = point.XboxLink;
                basicInfo.XboxPrice = point.XboxPrice;
                basicInfo.PlayStationLink = point.PlayStationLink;
                basicInfo.PlayStationPrice = point.PlayStationPrice;
                basicInfo.OriginLink = point.OriginLink;
                basicInfo.OriginPrice = point.OriginPrice;
                basicInfo.WindowsStoreLink = point.WindowsStoreLink;
                basicInfo.WindowsStorePrice = point.WindowsStorePrice;
                basicInfo.AppStoreLink = point.AppStoreLink;
                basicInfo.AppStorePrice = point.AppStorePrice;
                basicInfo.GooglePlayLink = point.GooglePlayLink;
                basicInfo.GooglePlayPrice = point.GooglePlayPrice;
                basicInfo.GogLink = point.GogLink;
                basicInfo.GogPrice = point.GogPrice;
                basicInfo.BattleNetLink = point.BattleNetLink;
                basicInfo.BattleNetPrice = point.BattleNetPrice;

                #endregion
            }
            else
            {
                var childPoints = await dbContext.PointRelationships
                    .Where(r => r.TargetPointId == point.Id)
                    .GroupBy(r => r.SourcePointId)
                    .Select(g => g.Key)
                    .ToListAsync();
                basicInfo.ProductCount = childPoints.Count;
                double totalScore = 0;
                var validRatingCount = 0;
                foreach (var childPointId in childPoints)
                {
                    var rating = (await cachedData.Points.GetRatingsAsync(childPointId)).AverageRating;
                    if (rating == null) continue;
                    totalScore += rating.Value;
                    validRatingCount++;
                }
                if (validRatingCount > 0)
                    basicInfo.AverageRating = Math.Round(totalScore/validRatingCount, 1);
            }
            if (point.Type == PointType.Game && point.SteamAppId != null)
            {
                SteamCrawlerProvider.UpdatePointPrice(point.Id);

                basicInfo.TotalPlayedTime = string.IsNullOrWhiteSpace(currentUserId)
                    ? null
                    : (await dbContext.UserSteamGameRecords
                        .Where(r => r.UserId == currentUserId && r.SteamAppId == point.SteamAppId)
                        .SingleOrDefaultAsync())?.TotalPlayedTime;

                basicInfo.KeylolAveragePlayedTime = Math.Round(await dbContext.UserSteamGameRecords
                    .Where(r => r.SteamAppId == point.SteamAppId)
                    .Select(r => r.TotalPlayedTime)
                    .DefaultIfEmpty()
                    .AverageAsync(), 1);

                basicInfo.InLibrary = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, point.SteamAppId.Value);
            }
            return basicInfo;
        }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

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

        /// <summary>
        /// 据点类型
        /// </summary>
        public PointType? Type { get; set; }

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
        public List<PointBasicInfo> Categories { get; set; }

        /// <summary>
        /// 厂商
        /// </summary>
        public List<PointBasicInfo> Vendors { get; set; }

        /// <summary>
        /// 标题封面
        /// </summary>
        public string TitleCoverImage { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

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
        public int? OneStarCount { get; set; }

        /// <summary>
        /// 二星评分个数
        /// </summary>
        public int? TwoStarCount { get; set; }

        /// <summary>
        /// 三星评分个数
        /// </summary>
        public int? ThreeStarCount { get; set; }

        /// <summary>
        /// 四星评分个数
        /// </summary>
        public int? FourStarCount { get; set; }

        /// <summary>
        /// 五星评分个数
        /// </summary>
        public int? FiveStarCount { get; set; }

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
        public string UplayPrice { get; set; }

        /// <summary>
        /// Xbox 链接
        /// </summary>
        public string XboxLink { get; set; }

        /// <summary>
        /// Xbox 价格
        /// </summary>
        public string XboxPrice { get; set; }

        /// <summary>
        /// PlayStation 链接
        /// </summary>
        public string PlayStationLink { get; set; }

        /// <summary>
        /// PlayStation 价格
        /// </summary>
        public string PlayStationPrice { get; set; }

        /// <summary>
        /// Origin 链接
        /// </summary>
        public string OriginLink { get; set; }

        /// <summary>
        /// Origin 价格
        /// </summary>
        public string OriginPrice { get; set; }

        /// <summary>
        /// Windows Store 链接
        /// </summary>
        public string WindowsStoreLink { get; set; }

        /// <summary>
        /// Windows Store 价格
        /// </summary>
        public string WindowsStorePrice { get; set; }

        /// <summary>
        /// App Store 链接
        /// </summary>
        public string AppStoreLink { get; set; }

        /// <summary>
        /// App Store 价格
        /// </summary>
        public string AppStorePrice { get; set; }

        /// <summary>
        /// Google Play 链接
        /// </summary>
        public string GooglePlayLink { get; set; }

        /// <summary>
        /// Google Play 价格
        /// </summary>
        public string GooglePlayPrice { get; set; }

        /// <summary>
        /// Gog 链接
        /// </summary>
        public string GogLink { get; set; }

        /// <summary>
        /// GOG 价格
        /// </summary>
        public string GogPrice { get; set; }

        /// <summary>
        /// 战网链接
        /// </summary>
        public string BattleNetLink { get; set; }

        /// <summary>
        /// 战网价格
        /// </summary>
        public string BattleNetPrice { get; set; }

        #endregion

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 旗下作品数
        /// </summary>
        public int? ProductCount { get; set; }

        /// <summary>
        /// Feed ID
        /// </summary>
        public long? FeedId { get; set; }
    }
}