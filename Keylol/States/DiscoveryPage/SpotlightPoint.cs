using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// 精选据点列表
    /// </summary>
    public class SpotlightPointList : List<SpotlightPoint>
    {
        private SpotlightPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="SpotlightPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightPointList"/></returns>
        public static async Task<SpotlightPointList> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == SpotlightPointStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                orderby feed.Id descending
                select new
                {
                    point.Id,
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
                    point.PlayStationPrice
                }).Take(30).ToListAsync();
            var result = new SpotlightPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new SpotlightPoint
                {
                    Id = p.Id,
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
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                            SubscriptionTargetType.Point),
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibrary(currentUserId, p.SteamAppId.Value)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 精选据点
    /// </summary>
    public class SpotlightPoint
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

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
        /// 当前用户是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }
    }
}