using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Entrance.Discovery
{
    /// <summary>
    /// 精选据点列表
    /// </summary>
    public class SpotlightPointList : List<PointBasicInfo>
    {
        private SpotlightPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取精选据点列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dbContext"></param>
        /// <param name="cachedData"></param>
        /// <returns></returns>
        public static async Task<SpotlightPointList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, 10, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="SpotlightPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordPerPage">每页个数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightPointList"/></returns>
        public static async Task<SpotlightPointList> CreateAsync(string currentUserId, int page, int recordPerPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == SpotlightPointStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                orderby feed.Id descending
                select new
                {
                    FeedId = feed.Id,
                    point.Id,
                    point.Type,
                    point.IdCode,
                    point.AvatarImage,
                    point.EnglishName,
                    point.ChineseName,
                    point.SteamAppId,
                    point.SteamPrice,
                    point.SteamDiscountedPrice,
                    point.SonkwoProductId,
                    point.SonkwoPrice,
                    point.SonkwoDiscountedPrice,
                    point.UplayLink,
                    point.UplayPrice,
                    point.XboxLink,
                    point.XboxPrice,
                    point.PlayStationLink,
                    point.PlayStationPrice,
                    point.OriginLink,
                    point.OriginPrice,
                    point.WindowsStoreLink,
                    point.WindowsStorePrice,
                    point.AppStoreLink,
                    point.AppStorePrice,
                    point.GooglePlayLink,
                    point.GooglePlayPrice,
                    point.GogLink,
                    point.GogPrice,
                    point.BattleNetLink,
                    point.BattleNetPrice
                }).TakePage(page, recordPerPage).ToListAsync();
            var result = new SpotlightPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new PointBasicInfo
                {
                    FeedId = p.FeedId,
                    Id = p.Id,
                    Type = p.Type,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    EnglishName = p.EnglishName,
                    ChineseName = p.ChineseName,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    SteamAppId = p.SteamAppId,
                    SteamPrice = p.SteamPrice,
                    SteamDiscountedPrice = p.SteamDiscountedPrice,
                    SonkwoProductId = p.SonkwoProductId,
                    SonkwoPrice = p.SonkwoPrice,
                    SonkwoDiscountedPrice = p.SonkwoDiscountedPrice,
                    UplayLink = p.UplayLink,
                    UplayPrice = p.UplayPrice,
                    XboxLink = p.XboxLink,
                    XboxPrice = p.XboxPrice,
                    PlayStationLink = p.PlayStationLink,
                    PlayStationPrice = p.PlayStationPrice,
                    OriginLink = p.OriginLink,
                    OriginPrice = p.OriginPrice,
                    WindowsStoreLink = p.WindowsStoreLink,
                    WindowsStorePrice = p.WindowsStorePrice,
                    AppStoreLink = p.AppStoreLink,
                    AppStorePrice = p.AppStorePrice,
                    GooglePlayLink = p.GooglePlayLink,
                    GooglePlayPrice = p.GooglePlayPrice,
                    GogLink = p.GogLink,
                    GogPrice = p.GogPrice,
                    BattleNetLink = p.BattleNetLink,
                    BattleNetPrice = p.BattleNetPrice,
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                            SubscriptionTargetType.Point),
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, p.SteamAppId.Value)
                });
            }
            return result;
        }
    }
}