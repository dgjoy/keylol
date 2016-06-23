using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Content.Activity
{
    /// <summary>
    /// 动态评论列表
    /// </summary>
    public class ActivityCommentList : List<ActivityComment>
    {
        private const int RecordsPerPage = 10;

        private ActivityCommentList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定动态的评论列表
        /// </summary>
        /// <param name="activityId">动态 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ActivityCommentList"/></returns>
        public static async Task<ActivityCommentList> Get(string activityId, int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            var activity = await dbContext.Activities
                .Include(a => a.Author)
                .Include(a => a.TargetPoint)
                .Where(a => a.Id == activityId)
                .SingleOrDefaultAsync();

            if (activity == null)
                return new ActivityCommentList(0);

            return (await CreateAsync(activity, page, StateTreeHelper.GetCurrentUserId(),
                StateTreeHelper.GetCurrentUser().IsInRole(KeylolRoles.Operator), false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="ActivityCommentList"/>
        /// </summary>
        /// <param name="activity">动态对象</param>
        /// <param name="page">分页页码</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="isOperator">当前登录用户是否为运维职员</param>
        /// <param name="returnMeta">是否返回元数据（总页数、总评论数）</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="ActivityCommentList"/>， Item2 表示总评论数，Item3 表示总页数</returns>
        public static async Task<Tuple<ActivityCommentList, int, int>> CreateAsync(Models.Activity activity,
            int page, string currentUserId, bool isOperator, bool returnMeta, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            if (activity.Archived != ArchivedState.None && currentUserId != activity.AuthorId && !isOperator)
                return new Tuple<ActivityCommentList, int, int>(new ActivityCommentList(0), 0, 0);

            var queryResult = await (from comment in dbContext.ActivityComments
                where comment.ActivityId == activity.Id
                orderby comment.Sid
                select new
                {
                    AuthorIdCode = comment.Commentator.IdCode,
                    AuthorAvatarImage = comment.Commentator.AvatarImage,
                    AuthorUserName = comment.Commentator.UserName,
                    AuthorId = comment.CommentatorId,
                    comment.Id,
                    comment.PublishTime,
                    comment.SidForActivity,
                    comment.Content,
                    comment.Archived,
                    comment.Warned
                }).TakePage(page, RecordsPerPage).ToListAsync();

            var result = new ActivityCommentList(queryResult.Count);
            foreach (var c in queryResult)
            {
                var activityComment = new ActivityComment
                {
                    Id = c.Id,
                    SidForActivity = c.SidForActivity,
                    Archived = c.Archived != ArchivedState.None
                };
                // ReSharper disable once PossibleInvalidOperationException
                if (!activityComment.Archived.Value || currentUserId == c.AuthorId || isOperator)
                {
                    activityComment.AuthorIdCode = c.AuthorIdCode;
                    activityComment.AuthorAvatarImage = c.AuthorAvatarImage;
                    activityComment.AuthorUserName = c.AuthorUserName;
                    activityComment.AuthorPlayedTime = activity.TargetPoint.SteamAppId == null
                        ? null
                        : (await dbContext.UserSteamGameRecords
                            .Where(r => r.UserId == c.AuthorId && r.SteamAppId == activity.TargetPoint.SteamAppId)
                            .SingleOrDefaultAsync())?.TotalPlayedTime;
                    activityComment.LikeCount =
                        await cachedData.Likes.GetTargetLikeCountAsync(c.Id, LikeTargetType.ActivityComment);
                    activityComment.Liked = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Likes.IsLikedAsync(currentUserId, c.Id, LikeTargetType.ActivityComment);
                    activityComment.PublishTime = c.PublishTime;
                    activityComment.Content = c.Content;
                    activityComment.Warned = c.Warned;
                }
                result.Add(activityComment);
            }
            var count = await cachedData.ActivityComments.GetActivityCommentCountAsync(activity.Id);
            return new Tuple<ActivityCommentList, int, int>(result,
                count,
                count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1);
        }

        /// <summary>
        /// 创建 <see cref="ActivityCommentList"/>
        /// </summary>
        /// <param name="activityId">动态 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="before">起始位置</param>
        /// <param name="take">获取数量</param>
        /// <param name="isOperator">当前登录用户是否为运维职员</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ActivityCommentList"/></returns>
        public static async Task<ActivityCommentList> CreateAsync(string activityId, string currentUserId,
            int before, int take, bool isOperator, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var activity = await dbContext.Activities.FindAsync(activityId);
            if (activity == null ||
                (activity.Archived != ArchivedState.None && currentUserId != activity.AuthorId && !isOperator))
                return new ActivityCommentList(0);

            var queryResult = await (from comment in dbContext.ActivityComments
                where comment.ActivityId == activity.Id && comment.Sid < before
                orderby comment.Sid descending
                select new
                {
                    AuthorIdCode = comment.Commentator.IdCode,
                    AuthorAvatarImage = comment.Commentator.AvatarImage,
                    AuthorUserName = comment.Commentator.UserName,
                    AuthorId = comment.CommentatorId,
                    comment.Id,
                    comment.PublishTime,
                    comment.SidForActivity,
                    comment.Content,
                    comment.Archived,
                    comment.Warned
                }).Take(take).ToListAsync();

            var result = new ActivityCommentList(queryResult.Count);
            foreach (var c in queryResult)
            {
                var activityComment = new ActivityComment
                {
                    Id = c.Id,
                    SidForActivity = c.SidForActivity,
                    Archived = c.Archived != ArchivedState.None
                };
                // ReSharper disable once PossibleInvalidOperationException
                if (!activityComment.Archived.Value || currentUserId == c.AuthorId || isOperator)
                {
                    activityComment.AuthorIdCode = c.AuthorIdCode;
                    activityComment.AuthorAvatarImage = c.AuthorAvatarImage;
                    activityComment.AuthorUserName = c.AuthorUserName;
                    activityComment.LikeCount =
                        await cachedData.Likes.GetTargetLikeCountAsync(c.Id, LikeTargetType.ActivityComment);
                    activityComment.Liked = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Likes.IsLikedAsync(currentUserId, c.Id, LikeTargetType.ActivityComment);
                    activityComment.PublishTime = c.PublishTime;
                    activityComment.Content = c.Content;
                    activityComment.Warned = c.Warned;
                }
                result.Add(activityComment);
            }
            result.Reverse();
            return result;
        }
    }

    /// <summary>
    /// 动态评论
    /// </summary>
    public class ActivityComment
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

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
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 楼层号
        /// </summary>  
        public int SidForActivity { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否被封存
        /// </summary>
        public bool? Archived { get; set; }

        /// <summary>
        /// 是否被警告
        /// </summary>
        public bool? Warned { get; set; }
    }
}