using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Aggregation.Point.Frontpage
{
    /// <summary>
    /// 近畿据点列表
    /// </summary>
    public class SimilarPointList : List<SimilarPoint>
    {
        /// <summary>
        /// 获取近畿据点列表
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SimilarPointList"/></returns>
        public static async Task<SimilarPointList> Get(string pointId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(pointId, StateTreeHelper.CurrentUser().Identity.GetUserId(), page,
                dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="SimilarPointList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="currentUserId">当点登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SimilarPointList"/></returns>
        public static async Task<SimilarPointList> CreateAsync(string pointId, string currentUserId, int page,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from relationship in dbContext.PointRelationships
                where dbContext.PointRelationships.Where(r => r.SourcePointId == pointId).Select(r => r.TargetPointId)
                    .Contains(relationship.TargetPointId) && relationship.SourcePointId != pointId
                group 1 by relationship.SourcePoint
                into g
                orderby g.Count() descending
                select new
                {
                    g.Key.Id,
                    g.Key.IdCode,
                    g.Key.AvatarImage,
                    g.Key.ChineseName,
                    g.Key.EnglishName,
                    g.Key.TitleCoverImage,
                    g.Key.SteamAppId
                }).TakePage(page, 8).ToListAsync();

            var result = new SimilarPointList();
            foreach (var p in queryResult)
            {
                result.Add(new SimilarPoint
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
                        : await cachedData.Users.IsSteamAppInLibrary(currentUserId, p.SteamAppId.Value)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 近畿据点
    /// </summary>
    public class SimilarPoint
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
    }
}