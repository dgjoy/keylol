using System;
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

namespace Keylol.States.PointsPage
{
    /// <summary>
    /// 最近有动态的据点列表
    /// </summary>
    public class RecentPointList : List<RecentPoint>
    {
        private const int RecordsPerPage = 12;

        private RecentPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定页码的最近有动态的据点列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>最新文章列表</returns>
        public static async Task<RecentPointList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return (await CreateAsync(StateTreeHelper.CurrentUser().Identity.GetUserId(), page, false,
                dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="RecentPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="returnPageCount">是否返回总页数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="RecentPointList"/>，Item2 表示总页数</returns>
        public static async Task<Tuple<RecentPointList, int>> CreateAsync(string currentUserId, int page,
            bool returnPageCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from point in dbContext.Points
                where point.Type == PointType.Game || point.Type == PointType.Hardware
                orderby point.LastActivityTime descending
                select point;
            var queryResult = await conditionQuery.Select(p => new
            {
                TotalCount = returnPageCount ? conditionQuery.Count() : 1,
                p.Id,
                p.IdCode,
                p.AvatarImage,
                p.ChineseName,
                p.EnglishName,
                p.TitleCoverImage,
                p.SteamAppId
            }).TakePage(page, RecordsPerPage).ToListAsync();

            var result = new RecentPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new RecentPoint
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
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<RecentPointList, int>(
                result,
                (int) Math.Ceiling(firstRecord?.TotalCount/(double) RecordsPerPage ?? 1));
        }
    }

    /// <summary>
    /// 最近有动态的据点
    /// </summary>
    public class RecentPoint
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
        /// 当前用户是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }
    }
}