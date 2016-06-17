using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;
using Keylol.States.Aggregation.Point.BasicInfo;
using Keylol.StateTreeManager;
using SimplePoint = Keylol.States.Content.Article.SimplePoint;

namespace Keylol.States.Content.Activity
{
    /// <summary>
    /// 内容 - 动态
    /// </summary>
    public class ActivityPage
    {
        /// <summary>
        /// 获取一条动态
        /// </summary>
        /// <param name="authorIdCode">作者识别码</param>
        /// <param name="sidForAuthor">动态在作者名下的序号</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ActivityPage"/></returns>
        public static async Task<ActivityPage> Get(string authorIdCode, int sidForAuthor,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(authorIdCode, sidForAuthor, StateTreeHelper.GetCurrentUserId(),
                StateTreeHelper.GetCurrentUser().IsInRole(KeylolRoles.Operator), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="ActivityPage"/>
        /// </summary>
        /// <param name="authorIdCode">作者识别码</param>
        /// <param name="sidForAuthor">动态在作者名下的序号</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="isOperator">当前登录用户是否为运维职员</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ActivityPage"/></returns>
        public static async Task<ActivityPage> CreateAsync(string authorIdCode, int sidForAuthor, string currentUserId,
            bool isOperator, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var activityPage = new ActivityPage();

            var activity = await dbContext.Activities
                .Include(a => a.Author)
                .Include(a => a.TargetPoint)
                .Where(a => a.Author.IdCode == authorIdCode && a.SidForAuthor == sidForAuthor)
                .SingleOrDefaultAsync();

            if (activity == null)
                return activityPage;

            activityPage.Archived = activity.Archived != ArchivedState.None;
            if (activityPage.Archived.Value && currentUserId != activity.AuthorId && !isOperator)
                return activityPage;

            activityPage.PointBasicInfo =
                await BasicInfo.CreateAsync(currentUserId, activity.TargetPoint, dbContext, cachedData);
            activityPage.AuthorId = activity.Author.Id;
            activityPage.AuthorIdCode = activity.Author.IdCode;
            activityPage.AuthorAvatarImage = activity.Author.AvatarImage;
            activityPage.AuthorUserName = activity.Author.UserName;
            activityPage.AuthorPlayedTime = activity.TargetPoint.SteamAppId == null
                ? null
                : (await dbContext.UserSteamGameRecords
                    .Where(r => r.UserId == activity.AuthorId && r.SteamAppId == activity.TargetPoint.SteamAppId)
                    .SingleOrDefaultAsync())?.TotalPlayedTime;
            activityPage.AuthorFriendCount = await cachedData.Subscriptions.GetFriendCountAsync(activity.AuthorId);
            activityPage.AuthorSubscriptionCount =
                await cachedData.Subscriptions.GetSubscriptionCountAsync(activity.AuthorId);
            activityPage.AuthorSubscriberCount =
                await cachedData.Subscriptions.GetSubscriberCountAsync(activity.AuthorId, SubscriptionTargetType.User);
            activityPage.AuthorSteamProfileName = activity.Author.SteamProfileName;
            activityPage.AuthorIsSubscribed = string.IsNullOrWhiteSpace(currentUserId)
                ? (bool?) null
                : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, activity.AuthorId,
                    SubscriptionTargetType.User);
            activityPage.Id = activity.Id;
            activityPage.PublishTime = activity.PublishTime;
            activityPage.Rejected = activity.Rejected;
            activityPage.Warned = activity.Warned;
            activityPage.Rating = activity.Rating;
            activityPage.Content = activity.Content;
            activityPage.CoverImage = activity.CoverImage;
            var attachedPointIds = Helpers.SafeDeserialize<List<string>>(activity.AttachedPoints) ?? new List<string>();
            activityPage.AttachedPoints = (from id in attachedPointIds
                join point in await (from point in dbContext.Points
                    where attachedPointIds.Contains(point.Id)
                    select new
                    {
                        point.Id,
                        point.IdCode,
                        point.AvatarImage,
                        point.ChineseName,
                        point.EnglishName
                    }).ToListAsync() on id equals point.Id
                select new SimplePoint
                {
                    Id = point.Id,
                    IdCode = point.IdCode,
                    AvatarImage = point.AvatarImage,
                    ChineseName = point.ChineseName,
                    EnglishName = point.EnglishName
                }).ToList();
            var comments = await ActivityCommentList.CreateAsync(activity, 1, currentUserId, isOperator, true,
                dbContext, cachedData);
            activityPage.CommentCount = comments.Item2;
            activityPage.CommentPageCount = comments.Item3;
            activityPage.Comments = comments.Item1;
            activityPage.LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(activity.Id, LikeTargetType.Article);
            return activityPage;
        }

        /// <summary>
        /// 据点基本信息
        /// </summary>
        public BasicInfo PointBasicInfo { get; set; }

        /// <summary>
        /// 作者 ID
        /// </summary>
        public string AuthorId { get; set; }

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
        /// 作者好友数
        /// </summary>
        public long? AuthorFriendCount { get; set; }

        /// <summary>
        /// 作者订阅数
        /// </summary>
        public long? AuthorSubscriptionCount { get; set; }

        /// <summary>
        /// 作者听众数
        /// </summary>
        public long? AuthorSubscriberCount { get; set; }

        /// <summary>
        /// 作者 Steam 昵称
        /// </summary>
        public string AuthorSteamProfileName { get; set; }

        /// <summary>
        /// 是否已订阅作者
        /// </summary>
        public bool? AuthorIsSubscribed { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 是否被封存
        /// </summary>
        public bool? Archived { get; set; }

        /// <summary>
        /// 是否被退稿
        /// </summary>
        public bool? Rejected { get; set; }

        /// <summary>
        /// 是否被警告
        /// </summary>
        public bool? Warned { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 关联投稿据点列表
        /// </summary>
        public List<SimplePoint> AttachedPoints { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 评论总页数
        /// </summary>
        public int? CommentPageCount { get; set; }

        /// <summary>
        /// 评论列表
        /// </summary>
        public ActivityCommentList Comments { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }
    }
}