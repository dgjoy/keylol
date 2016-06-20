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

namespace Keylol.States.Aggregation.Point.Frontpage
{
    /// <summary>
    /// 简评列表
    /// </summary>
    public class BriefReviewList : List<BriefReview>
    {
        private const int RecordsPerPage = 5;

        private BriefReviewList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取简评列表
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="BriefReviewList"/></returns>
        public static async Task<BriefReviewList> Get(string pointId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.FindAsync(pointId);
            if (point == null)
                return new BriefReviewList(0);
            return (await CreateAsync(point, page, false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="BriefReviewList"/>
        /// </summary>
        /// <param name="point">据点对象</param>
        /// <param name="page">分页页码</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="BriefReviewList"/>，Item2 表示总数，Item3 表示总页数</returns>
        public static async Task<Tuple<BriefReviewList, int, int>> CreateAsync(Models.Point point, int page,
            bool returnCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from activity in dbContext.Activities
                where activity.TargetPointId == point.Id && activity.Rating != null &&
                      activity.Archived == ArchivedState.None
                orderby dbContext.Likes
                    .Count(l => l.TargetId == activity.Id && l.TargetType == LikeTargetType.Activity) descending
                select new
                {
                    Count = returnCount
                        ? dbContext.Activities.Count(
                            a => a.TargetPointId == point.Id && a.Rating != null && a.Archived == ArchivedState.None)
                        : 1,
                    activity.Id,
                    activity.AuthorId,
                    AuthorIdCode = activity.Author.IdCode,
                    AuthorAvatarImage = activity.Author.AvatarImage,
                    AuthorUserName = activity.Author.UserName,
                    activity.SidForAuthor,
                    activity.Rating,
                    activity.Content
                }).TakePage(page, RecordsPerPage).ToListAsync();

            var result = new BriefReviewList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new BriefReview
                {
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    AuthorPlayedTime = point.SteamAppId == null
                        ? null
                        : (await dbContext.UserSteamGameRecords
                            .Where(r => r.UserId == a.AuthorId && r.SteamAppId == point.SteamAppId)
                            .SingleOrDefaultAsync())?.TotalPlayedTime,
                    SidForAuthor = a.SidForAuthor,
                    Rating = a.Rating,
                    LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Activity),
                    Content = a.Content
                });
            }
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<BriefReviewList, int, int>(result,
                firstRecord?.Count ?? 0,
                (int) Math.Ceiling(firstRecord?.Count/(double) RecordsPerPage ?? 1));
        }
    }

    /// <summary>
    /// 简评
    /// </summary>
    public class BriefReview
    {
        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorAvatarImage { get; set; }

        /// <summary>
        /// 作者用户名
        /// </summary>
        public string AuthorUserName { get; set; }

        /// <summary>
        /// 作者在档时间
        /// </summary>
        public double? AuthorPlayedTime { get; set; }

        /// <summary>
        /// 在作者名下的序号
        /// </summary>
        public int? SidForAuthor { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}