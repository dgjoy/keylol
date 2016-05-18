using System.Collections.Generic;

namespace Keylol.States.Aggregation.Point.BasicInfo
{
    /// <summary>
    /// 据点基础信息
    /// </summary>
    public class BasicInfo
    {
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
        public double? PlayedTime { get; set; }

        /// <summary>
        /// 其乐用户平均在档时间
        /// </summary>
        public double AveragePlayedTime { get; set; }

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