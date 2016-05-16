using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.PointsPage
{
    /// <summary>
    /// 哨所据点列表
    /// </summary>
    public class OutpostPointList : List<OutpostPoint>
    {
        private OutpostPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="OutpostPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="OutpostPointList"/></returns>
        public static async Task<OutpostPointList> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == OutpostStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                orderby feed.Id descending
                select new
                {
                    point.Id,
                    point.IdCode,
                    point.AvatarImage,
                    point.ChineseName,
                    point.EnglishName,
                    point.TitleCoverImage,
                    point.MultiPlayer,
                    point.SinglePlayer,
                    point.Coop,
                    point.CaptionsAvailable,
                    point.CommentaryAvailable,
                    point.IncludeLevelEditor,
                    point.Achievements,
                    point.Cloud,
                    point.LocalCoop,
                    point.SteamTradingCards,
                    point.SteamWorkshop,
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
                })
                .Take(15)
                .ToListAsync();
            var result = new OutpostPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new OutpostPoint
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    TitleCoverImage = p.TitleCoverImage,
                    MultiPlayer = p.MultiPlayer,
                    SinglePlayer = p.SinglePlayer,
                    Coop = p.Coop,
                    CaptionsAvailable = p.CaptionsAvailable,
                    CommentaryAvailable = p.CommentaryAvailable,
                    IncludeLevelEditor = p.IncludeLevelEditor,
                    Achievements = p.Achievements,
                    Cloud = p.Cloud,
                    LocalCoop = p.LocalCoop,
                    SteamTradingCards = p.SteamTradingCards,
                    SteamWorkshop = p.SteamWorkshop,
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
    /// 哨所据点
    /// </summary>
    public class OutpostPoint
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
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

        /// <summary>
        /// 标题封面
        /// </summary>
        public string TitleCoverImage { get; set; }

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

        #endregion

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