using System;
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

namespace Keylol.States.Entrance.Points
{
    /// <summary>
    /// 最近有动态的据点列表
    /// </summary>
    public class RecentPointList : List<PointBasicInfo>
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
            return (await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, false,
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
                Count = returnPageCount ? conditionQuery.Count() : 1,
                p.Id,
                p.Type,
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
                result.Add(new PointBasicInfo
                {
                    Id = p.Id,
                    Type = p.Type,
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
            return new Tuple<RecentPointList, int>(
                result,
                (int) Math.Ceiling(firstRecord?.Count/(double) RecordsPerPage ?? 1));
        }
    }
}