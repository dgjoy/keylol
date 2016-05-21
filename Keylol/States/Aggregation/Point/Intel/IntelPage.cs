using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.Point.Intel
{
    /// <summary>
    /// 聚合 - 据点 - 情报
    /// </summary>
    public class IntelPage
    {
        /// <summary>
        /// 创建 <see cref="IntelPage"/>
        /// </summary>
        /// <param name="point">据点对象</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="IntelPage"/></returns>
        public static async Task<IntelPage> CreateAsync(Models.Point point, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var intelPage = new IntelPage();
            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                intelPage.VendorPoints = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          (relationship.Relationship == PointRelationshipType.Developer ||
                           relationship.Relationship == PointRelationshipType.Manufacturer ||
                           relationship.Relationship == PointRelationshipType.Publisher ||
                           relationship.Relationship == PointRelationshipType.Reseller)
                    group relationship.Relationship by new
                    {
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.AvatarImage,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new VendorPoint
                    {
                        IdCode = p.Key.IdCode,
                        AvatarImage = p.Key.AvatarImage,
                        ChineseName = p.Key.ChineseName,
                        EnglishName = p.Key.EnglishName,
                        Roles = string.Join("、", p.Select(r =>
                        {
                            switch (r)
                            {
                                case PointRelationshipType.Developer:
                                    return "开发商";
                                case PointRelationshipType.Publisher:
                                    return "发行商";
                                case PointRelationshipType.Manufacturer:
                                    return "制造商";
                                case PointRelationshipType.Reseller:
                                    return "代理商";
                                default:
                                    return r.ToString();
                            }
                        }))
                    })
                    .ToList();
            }
            if (point.Type == PointType.Game)
            {
                SteamCrawlerProvider.UpdateSteamSpyData(point.Id);
                intelPage.PublishDate = point.PublishDate;
                intelPage.PointCreateTime = point.CreateTime;
                intelPage.PreOrderDate = point.PreOrderDate;
                intelPage.ReleaseDate = point.ReleaseDate;
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
            return intelPage;
        }

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
    }
}