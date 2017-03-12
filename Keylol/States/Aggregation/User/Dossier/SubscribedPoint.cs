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

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 订阅的据点列表
    /// </summary>
    public class SubscribedPointList : List<PointBasicInfo>
    {
        private SubscribedPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定用户订阅的据点列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SubscribedPointList"/></returns>
        public static async Task<SubscribedPointList> Get(string userId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return (await CreateAsync(StateTreeHelper.GetCurrentUserId(), userId, page, 30, false, dbContext,
                cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="SubscribedPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="SubscribedPointList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<SubscribedPointList, int>> CreateAsync(string currentUserId, string userId,
            int page, int recordsPerPage, bool returnCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from subscription in dbContext.Subscriptions
                where subscription.SubscriberId == userId && subscription.TargetType == SubscriptionTargetType.Point
                select subscription;
            var queryResult = await (from subscription in conditionQuery
                join point in dbContext.Points on subscription.TargetId equals point.Id
                orderby subscription.Sid descending
                select new
                {
                    Count = returnCount ? conditionQuery.Count() : 1,
                    point.Type,
                    point.IdCode,
                    point.AvatarImage,
                    point.ChineseName,
                    point.EnglishName,
                    point.SteamAppId
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new SubscribedPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new PointBasicInfo
                {
                    Type = p.Type,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, p.SteamAppId.Value)
                });
            }
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<SubscribedPointList, int>(result, firstRecord?.Count ?? 0);
        }
    }
}