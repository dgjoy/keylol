using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Entrance.PointsPage
{
    /// <summary>
    /// 最近玩过的游戏对应据点列表
    /// </summary>
    public class RecentPlayedPointList : List<RecentPlayedPoint>
    {
        private RecentPlayedPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="RecentPlayedPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="RecentPlayedPointList"/>，Item2 表示第一个据点头部图</returns>
        public static async Task<Tuple<RecentPlayedPointList, string>> CreateAsync(string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (string.IsNullOrWhiteSpace(currentUserId)
                ? from point in dbContext.Points
                    where DbFunctions.DiffMonths(point.CreateTime, DateTime.Now) <= 3 &&
                          (point.Type == PointType.Game || point.Type == PointType.Hardware)
                    orderby dbContext.Subscriptions
                        .Count(s => s.TargetId == point.Id && s.TargetType == SubscriptionTargetType.Point) descending,
                        point.LastActivityTime descending
                    select new
                    {
                        point.Id,
                        point.IdCode,
                        point.ThumbnailImage,
                        point.ChineseName,
                        point.EnglishName,
                        TwoWeekPlayedTime = (double?) null
                    }
                : from record in dbContext.UserGameRecords
                    where record.UserId == currentUserId
                    join point in dbContext.Points on record.SteamAppId equals point.SteamAppId
                    where !dbContext.Subscriptions.Any(s =>
                        s.SubscriberId == currentUserId && s.TargetId == point.Id &&
                        s.TargetType == SubscriptionTargetType.Point)
                    orderby record.TwoWeekPlayedTime descending, record.LastPlayTime descending
                    select new
                    {
                        point.Id,
                        point.IdCode,
                        point.ThumbnailImage,
                        point.ChineseName,
                        point.EnglishName,
                        TwoWeekPlayedTime = (double?) record.TwoWeekPlayedTime
                    }
                ).Take(5).ToListAsync();
            var result = new RecentPlayedPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new RecentPlayedPoint
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    ThumbnailImage = p.ThumbnailImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    TwoWeekPlayedTime = p.TwoWeekPlayedTime
                });
            }
            var firstRecord = result.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ThumbnailImage));
            return new Tuple<RecentPlayedPointList, string>(
                result,
                firstRecord?.ThumbnailImage);
        }
    }

    /// <summary>
    /// 最近玩过的游戏对应据点
    /// </summary>
    public class RecentPlayedPoint
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
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

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
        /// 两周游戏时间
        /// </summary>
        public double? TwoWeekPlayedTime { get; set; }
    }
}