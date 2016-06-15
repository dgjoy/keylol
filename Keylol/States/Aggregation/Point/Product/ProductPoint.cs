using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.Point.Product
{
    /// <summary>
    /// 据点旗下产品列表
    /// </summary>
    public class ProductPointList : List<ProductPoint>
    {
        private ProductPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建满足指定据点关系的 <see cref="ProductPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="pointId">据点 ID</param>
        /// <param name="relationshipType">据点身份</param>
        /// <param name="take">获取数量，如果为 null 表示获取全部</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="ProductPointList"/>，Item2 表示总据点个数</returns>
        public static async Task<Tuple<ProductPointList, int>> CreateAsync(string currentUserId, string pointId,
            PointRelationshipType relationshipType, int? take, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from relationship in dbContext.PointRelationships
                where relationship.TargetPointId == pointId && relationship.Relationship == relationshipType
                select relationship;
            var query = conditionQuery.OrderByDescending(r => r.Sid).Select(r => new
            {
                TotalCount = conditionQuery.Count(),
                r.SourcePoint.Id,
                r.SourcePoint.IdCode,
                r.SourcePoint.AvatarImage,
                r.SourcePoint.ChineseName,
                r.SourcePoint.EnglishName,
                r.SourcePoint.TitleCoverImage,
                r.SourcePoint.SteamAppId
            });
            if (take != null)
                query = query.Take(() => take.Value);
            var queryResult = await query.ToListAsync();

            var result = new ProductPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new ProductPoint
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    TitleCoverImage = p.TitleCoverImage,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                            SubscriptionTargetType.Point),
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, p.SteamAppId.Value)
                });
            }
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<ProductPointList, int>(result, firstRecord?.TotalCount ?? 0);
        }

        /// <summary>
        /// 创建 <see cref="ProductPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="pointId">据点 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ProductPointList"/></returns>
        public static async Task<ProductPointList> CreateAsync(string currentUserId, string pointId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from relationship in dbContext.PointRelationships
                where relationship.TargetPointId == pointId
                group relationship.Relationship by new
                {
                    relationship.SourcePoint.Sid,
                    relationship.SourcePoint.Id,
                    relationship.SourcePoint.IdCode,
                    relationship.SourcePoint.AvatarImage,
                    relationship.SourcePoint.ChineseName,
                    relationship.SourcePoint.EnglishName,
                    relationship.SourcePoint.TitleCoverImage,
                    relationship.SourcePoint.SteamAppId
                }
                into g
                orderby g.Key.Sid descending
                select g)
                .ToListAsync();

            var result = new ProductPointList(queryResult.Count);
            foreach (var g in queryResult)
            {
                result.Add(new ProductPoint
                {
                    Id = g.Key.Id,
                    IdCode = g.Key.IdCode,
                    AvatarImage = g.Key.AvatarImage,
                    ChineseName = g.Key.ChineseName,
                    EnglishName = g.Key.EnglishName,
                    TitleCoverImage = g.Key.TitleCoverImage,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(g.Key.Id)).AverageRating,
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, g.Key.Id,
                            SubscriptionTargetType.Point),
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || g.Key.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, g.Key.SteamAppId.Value),
                    Roles = g.ToList()
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 据点旗下产品
    /// </summary>
    public class ProductPoint
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
        /// 标题封面
        /// </summary>
        public string TitleCoverImage { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }

        /// <summary>
        /// 与目标据点的关系
        /// </summary>
        public List<PointRelationshipType> Roles { get; set; }
    }
}