using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Aggregation.Point.BasicInfo;
using Keylol.States.Aggregation.Point.Editing;
using Keylol.States.Aggregation.Point.Frontpage;
using Keylol.States.Aggregation.Point.Intel;
using Keylol.States.Aggregation.Point.Timeline;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Aggregation.Point
{
    /// <summary>
    /// 聚合 - 据点层级
    /// </summary>
    public class PointLevel
    {
        /// <summary>
        /// 获取据点层级状态树
        /// </summary>
        /// <param name="entrance">要获取的页面</param>
        /// <param name="idCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointLevel"/></returns>
        public static async Task<PointLevel> Get(string entrance, string idCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.CurrentUser().Identity.GetUserId(),
                idCode, entrance.ToEnum<EntrancePage>(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="PointLevel"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="targetPage">要获取的页面</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetPage"/> 超出范围</exception>
        public static async Task<PointLevel> CreateAsync(string currentUserId, string pointIdCode,
            EntrancePage targetPage, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return null;
            var rating = await cachedData.Points.GetRatingsAsync(point.Id);
            var result = new PointLevel
            {
                BasicInfo = new BasicInfo.BasicInfo
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
                            relationship.TargetPoint.ChineseName
                        })
                        .ToListAsync())
                        .Select(p => new SimplePoint
                        {
                            IdCode = p.IdCode,
                            ChineseName = p.ChineseName
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
                    PlayedTime = (await dbContext.UserSteamGameRecords
                        .Where(r => r.UserId == currentUserId && r.SteamAppId == point.SteamAppId)
                        .SingleOrDefaultAsync())?.TotalPlayedTime,
                    AveragePlayedTime = await dbContext.UserSteamGameRecords
                        .Where(r => r.SteamAppId == point.SteamAppId)
                        .Select(r => r.TotalPlayedTime)
                        .DefaultIfEmpty()
                        .AverageAsync(),
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
                        : await cachedData.Users.IsSteamAppInLibrary(currentUserId, point.SteamAppId.Value)
                }
            };
            switch (targetPage)
            {
                case EntrancePage.Auto:
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        result.Frontpage = await FrontpagePage.CreateAsync(point, dbContext, cachedData);
                        result.Current = EntrancePage.Frontpage;
                    }
                    else
                    {
                        result.Current = EntrancePage.Timeline;
                    }
                    break;

                case EntrancePage.Frontpage:
                    result.Frontpage = await FrontpagePage.CreateAsync(point, dbContext, cachedData);
                    break;

                case EntrancePage.Intel:
                    break;

                case EntrancePage.Timeline:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetPage), targetPage, null);
            }
            return result;
        }

        /// <summary>
        /// 据点基础信息
        /// </summary>
        public BasicInfo.BasicInfo BasicInfo { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public EntrancePage? Current { get; set; }

        /// <summary>
        /// 扉页
        /// </summary>
        public FrontpagePage Frontpage { get; set; }

        /// <summary>
        /// 情报
        /// </summary>
        public IntelPage Intel { get; set; }

        /// <summary>
        /// 轨道
        /// </summary>
        public TimelinePage Timeline { get; set; }

        /// <summary>
        /// 编辑层级
        /// </summary>
        public EditingLevel Editing { get; set; }
    }

    /// <summary>
    /// 目标入口页
    /// </summary>
    public enum EntrancePage
    {
        /// <summary>
        /// 自动（根据登录状态）
        /// </summary>
        Auto,

        /// <summary>
        /// 扉页
        /// </summary>
        Frontpage,

        /// <summary>
        /// 情报
        /// </summary>
        Intel,

        /// <summary>
        /// 轨道
        /// </summary>
        Timeline
    }
}