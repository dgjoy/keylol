using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Aggregation.User.Dossier.Article;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.Dossier.Point
{
    /// <summary>
    /// 用户订阅的据点列表
    /// </summary>
    public class PointList : List<Point>
    {
        private PointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取用户订阅列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<PointList> Get(string userId, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(userId, page, recordsPerPage, dbContext, cachedData);
        }

        /// <summary>
        /// 创建用用户订阅列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<PointList> CreateAsync(string userId, int page, int recordsPerPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from subscription in dbContext.Subscriptions
                                 where subscription.SubscriberId == userId && subscription.TargetType == SubscriptionTargetType.Point
                                 select subscription;

            var queryResult = await (from subscription in conditionQuery
                                     join point in dbContext.Points on subscription.TargetId equals point.Id
                                     orderby subscription.Sid descending
                                     select new
                                     {
                                        point.Id,
                                        point.Type,
                                        point.IdCode,
                                        point.AvatarImage,
                                        point.ChineseName,
                                        point.EnglishName,
                                        point.SteamAppId
                                     }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new PointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new Point
                {
                    Type = p.Type,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    InLibrary = string.IsNullOrWhiteSpace(userId) || p.SteamAppId == null
                        ? (bool?)null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(userId, p.SteamAppId.Value),
                    Subscribed = string.IsNullOrWhiteSpace(userId)? await cachedData.Subscriptions.IsSubscribedAsync(userId, p.Id,
                            SubscriptionTargetType.Point)
                        : (bool?)null,
                    SubscriberCount = await cachedData.Subscriptions.GetSubscriberCountAsync(p.Id, SubscriptionTargetType.Point)
//                    ArticleCount =  p.ArticleCount,
//                    ActivityCount = p.ActivityCount ,
                });
            }

            return result;
        }
    }
    /// <summary>
    /// 用户订阅的据点
    /// </summary>
    public class Point
    {
        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string  IdCode { get; set; }

        /// <summary>
        /// 据点类型
        /// </summary>
        public PointType Type { get; set; }

        /// <summary>
        /// 读者数量
        /// </summary>
        public long? SubscriberCount { get; set; }

        /// <summary>
        /// 文章数
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态数
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 是否被订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否入库
        /// </summary>
        public bool? InLibrary { get; set; }
    }
}