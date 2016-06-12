using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;
using Keylol.States.Aggregation.Point.BasicInfo;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point.Intel
{
    /// <summary>
    /// 聚合 - 据点 - 情报
    /// </summary>
    public class IntelPage
    {
        /// <summary>
        /// 获取据点情报
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="IntelPage"/></returns>
        public static async Task<IntelPage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new IntelPage();
            return await CreateAsync(point, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="IntelPage"/>
        /// </summary>
        /// <param name="point">据点对象</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="IntelPage"/></returns>
        public static async Task<IntelPage> CreateAsync(Models.Point point, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var intelPage = new IntelPage();

            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                var vendorPointsGroup = await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          (relationship.Relationship == PointRelationshipType.Developer ||
                           relationship.Relationship == PointRelationshipType.Manufacturer ||
                           relationship.Relationship == PointRelationshipType.Publisher ||
                           relationship.Relationship == PointRelationshipType.Reseller)
                    group relationship.Relationship by new
                    {
                        relationship.TargetPoint.Id,
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync();
                intelPage.VendorPoints = vendorPointsGroup.Select(g => new VendorPoint
                {
                    IdCode = g.Key.IdCode,
                    AvatarImage = g.Key.AvatarImage,
                    ChineseName = g.Key.ChineseName,
                    EnglishName = g.Key.EnglishName,
                    Roles = string.Join("、", g.Select(r =>
                    {
                        switch (r)
                        {
                            case PointRelationshipType.Developer:
                                return "开发厂";
                            case PointRelationshipType.Publisher:
                                return "发行商";
                            case PointRelationshipType.Manufacturer:
                                return "制造厂";
                            case PointRelationshipType.Reseller:
                                return "代理商";
                            default:
                                return r.ToString();
                        }
                    }))
                }).ToList();
                var vendorIds = vendorPointsGroup.Select(g => g.Key.Id).ToList();
                var vendorPointStaffQueryResult = await (from staff in dbContext.PointStaff
                    where vendorIds.Contains(staff.PointId)
                    select new
                    {
                        staff.Staff.Id,
                        staff.Staff.HeaderImage,
                        staff.Staff.IdCode,
                        staff.Staff.AvatarImage,
                        staff.Staff.UserName,
                        PointChineseName = staff.Point.ChineseName,
                        PointEnglishName = staff.Point.EnglishName
                    }).ToListAsync();
                intelPage.VenderPointStaff = new List<VendorPointStaff>(vendorPointStaffQueryResult.Count);
                foreach (var u in vendorPointStaffQueryResult)
                {
                    intelPage.VenderPointStaff.Add(new VendorPointStaff
                    {
                        Id = u.Id,
                        HeaderImage = u.HeaderImage,
                        IdCode = u.IdCode,
                        AvatarImage = u.AvatarImage,
                        UserName = u.UserName,
                        PointChineseName = u.PointChineseName,
                        PointEnglishName = u.PointEnglishName,
                        IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                            ? (bool?) null
                            : await cachedData.Users.IsFriendAsync(currentUserId, u.Id),
                        Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                            ? (bool?) null
                            : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, u.Id,
                                SubscriptionTargetType.User)
                    });
                }
                intelPage.PointStaff = await PointStaffList.CreateAsync(point.Id, currentUserId, dbContext, cachedData);
                intelPage.GenrePoints = await GenrePointList.CreateAsync(point.Id, currentUserId, dbContext, cachedData);
                intelPage.TagPoints = await TagPointList.CreateAsync(point.Id, currentUserId, dbContext, cachedData);
            }
            if (point.Type == PointType.Game)
            {
                intelPage.Platforms = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Platform
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
                    .ToList();

                #region 特性属性

                intelPage.MultiPlayer = point.MultiPlayer ? true : (bool?)null;
                intelPage.SinglePlayer = point.SinglePlayer ? true : (bool?)null;
                intelPage.Coop = point.Coop ? true : (bool?)null;
                intelPage.CaptionsAvailable = point.CaptionsAvailable ? true : (bool?)null;
                intelPage.CommentaryAvailable = point.CommentaryAvailable ? true : (bool?)null;
                intelPage.IncludeLevelEditor = point.IncludeLevelEditor ? true : (bool?)null;
                intelPage.Achievements = point.Achievements ? true : (bool?)null;
                intelPage.Cloud = point.Cloud ? true : (bool?)null;
                intelPage.LocalCoop = point.LocalCoop ? true : (bool?)null;
                intelPage.SteamTradingCards = point.SteamTradingCards ? true : (bool?)null;
                intelPage.SteamWorkshop = point.SteamWorkshop ? true : (bool?)null;
                intelPage.InAppPurchases = point.InAppPurchases ? true : (bool?)null;

                #endregion

                intelPage.ChineseAvailability = Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability);

                SteamCrawlerProvider.UpdateSteamSpyData(point.Id);

                intelPage.PublishDate = point.PublishDate;
                intelPage.PointCreateTime = point.CreateTime;
                intelPage.PreOrderDate = point.PreOrderDate;
                intelPage.ReleaseDate = point.ReleaseDate;

                if (point.SteamAppId != null)
                {
                    intelPage.OwnerCount = point.OwnerCount;
                    intelPage.OwnerCountVariance = point.OwnerCountVariance;
                    intelPage.TotalPlayerCount = point.TotalPlayerCount;
                    intelPage.TotalPlayerCountVariance = point.TotalPlayerCountVariance;
                    intelPage.TwoWeekPlayerCount = point.TwoWeekPlayerCount;
                    intelPage.TwoWeekPlayerCountVariance = point.TwoWeekPlayerCountVariance;
                    intelPage.Ccu = point.Ccu;
                    intelPage.AveragePlayedTime = point.AveragePlayedTime;
                    intelPage.TwoWeekAveragePlayedTime = point.TwoWeekAveragePlayedTime;
                    intelPage.MedianPlayedTime = point.MedianPlayedTime;
                }
            }
            return intelPage;
        }

        /// <summary>
        /// 平台
        /// </summary>
        public List<SimplePoint> Platforms { get; set; }

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
        /// 华语可用度
        /// </summary>
        public ChineseAvailability ChineseAvailability { get; set; }

        /// <summary>
        /// 厂商据点
        /// </summary>
        public List<VendorPoint> VendorPoints { get; set; }

        /// <summary>
        /// 公开日期
        /// </summary>
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// 据点创建时间
        /// </summary>
        public DateTime? PointCreateTime { get; set; }

        /// <summary>
        /// 预售日期
        /// </summary>
        public DateTime? PreOrderDate { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// 记录售出
        /// </summary>
        public int? OwnerCount { get; set; }

        /// <summary>
        /// 记录售出误差
        /// </summary>
        public int? OwnerCountVariance { get; set; }

        /// <summary>
        /// 玩家总数
        /// </summary>
        public int? TotalPlayerCount { get; set; }

        /// <summary>
        /// 玩家总数误差
        /// </summary>
        public int? TotalPlayerCountVariance { get; set; }

        /// <summary>
        /// 两周活跃玩家
        /// </summary>
        public int? TwoWeekPlayerCount { get; set; }

        /// <summary>
        /// 两周活跃玩家误差
        /// </summary>
        public int? TwoWeekPlayerCountVariance { get; set; }

        /// <summary>
        /// 是日同时在线
        /// </summary>
        public int? Ccu { get; set; }

        /// <summary>
        /// 全网人均在档（分钟）
        /// </summary>
        public int? AveragePlayedTime { get; set; }

        /// <summary>
        /// 全网两周人均在档（分钟）
        /// </summary>
        public int? TwoWeekAveragePlayedTime { get; set; }

        /// <summary>
        /// 全网人均在档中位数（分钟）
        /// </summary>
        public int? MedianPlayedTime { get; set; }

        /// <summary>
        /// 厂商据点职员列表
        /// </summary>
        public List<VendorPointStaff> VenderPointStaff { get; set; }

        /// <summary>
        /// 该据点职员列表
        /// </summary>
        public PointStaffList PointStaff { get; set; }

        /// <summary>
        /// 流派据点列表
        /// </summary>
        public GenrePointList GenrePoints { get; set; }

        /// <summary>
        /// 特性据点列表
        /// </summary>
        public TagPointList TagPoints { get; set; }
    }
}