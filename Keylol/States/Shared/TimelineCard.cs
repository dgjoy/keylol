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
using Keylol.States.Content.Activity;
using Keylol.StateTreeManager;

namespace Keylol.States.Shared
{
    /// <summary>
    /// 时间轴卡片列表
    /// </summary>
    public class TimelineCardList : List<TimelineCard>
    {
        /// <summary>
        /// 定位器名称
        /// </summary>
        public static string LocatorName() => "activityId";

        private TimelineCardList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="TimelineCardList"/>
        /// </summary>
        /// <param name="streamName">Feed 流名称</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="take">获取数量</param>
        /// <param name="ignoreRejected">忽略退稿状态</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="before">起始位置</param>
        /// <returns><see cref="TimelineCardList"/></returns>
        public static async Task<TimelineCardList> CreateAsync(string streamName, string currentUserId, int take,
            bool ignoreRejected, KeylolDbContext dbContext, CachedDataProvider cachedData, int before = int.MaxValue)
        {
            if (take > 50) take = 50;
            var result = new TimelineCardList(20);
            List<Feed> feeds;
            do
            {
                // ReSharper disable once AccessToModifiedClosure
                feeds = await dbContext.Feeds.Where(f => f.StreamName == streamName && f.Id < before)
                    .OrderByDescending(f => f.Id).Take(() => take).ToListAsync();
                foreach (var feed in feeds)
                {
                    var card = new TimelineCard
                    {
                        FeedId = feed.Id,
                        AttachedPoints = new List<PointBasicInfo>(1)
                    };
                    string targetPointId;
                    switch (feed.EntryType)
                    {
                        case FeedEntryType.ArticleId:
                        {
                            var a = await (from article in dbContext.Articles
                                where article.Id == feed.Entry
                                select new
                                {
                                    article.AuthorId,
                                    AuthorIdCode = article.Author.IdCode,
                                    AuthorAvatarImage = article.Author.AvatarImage,
                                    AuthorUserName = article.Author.UserName,
                                    article.PublishTime,
                                    article.SidForAuthor,
                                    article.CoverImage,
                                    article.Title,
                                    article.Subtitle,
                                    article.Rating,
                                    article.AttachedPoints,
                                    article.Archived,
                                    article.Rejected,
                                    PointId = article.TargetPointId,
                                    PointIdCode = article.TargetPoint.IdCode,
                                    PointAvatarImage = article.TargetPoint.AvatarImage,
                                    PointChineseName = article.TargetPoint.ChineseName,
                                    PointEnglishName = article.TargetPoint.EnglishName
                                }).SingleOrDefaultAsync();
                            if (a == null || a.Archived != ArchivedState.None || (!ignoreRejected && a.Rejected))
                                continue;
                            card.Author = new UserBasicInfo
                            {
                                IdCode = a.AuthorIdCode,
                                AvatarImage = a.AuthorAvatarImage,
                                UserName = a.AuthorUserName,
                                IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                                    ? (bool?) null
                                    : await cachedData.Users.IsFriendAsync(currentUserId, a.AuthorId)
                            };
                            card.PublishTime = a.PublishTime;
                            card.ContentType = TimelineCardContentType.Article;
                            card.ContentId = feed.Entry;
                            card.SidForAuthor = a.SidForAuthor;
                            card.CoverImage = a.CoverImage;
                            card.Title = a.Title;
                            card.Subtitle = a.Subtitle;
                            card.LikeCount =
                                await cachedData.Likes.GetTargetLikeCountAsync(feed.Entry, LikeTargetType.Article);
                            card.Liked = string.IsNullOrWhiteSpace(currentUserId)
                                ? (bool?) null
                                : await cachedData.Likes.IsLikedAsync(currentUserId, feed.Entry, LikeTargetType.Article);
                            card.CommentCount = await cachedData.ArticleComments.GetArticleCommentCountAsync(feed.Entry);
                            targetPointId = a.PointId;
                            card.AttachedPoints.Add(new PointBasicInfo
                            {
                                IdCode = a.PointIdCode,
                                AvatarImage = a.PointAvatarImage,
                                ChineseName = a.PointChineseName,
                                EnglishName = a.PointEnglishName
                            });
                            break;
                        }

                        case FeedEntryType.ActivityId:
                        {
                            var a = await (from activity in dbContext.Activities
                                where activity.Id == feed.Entry
                                select new
                                {
                                    activity.AuthorId,
                                    AuthorIdCode = activity.Author.IdCode,
                                    AuthorAvatarImage = activity.Author.AvatarImage,
                                    AuthorUserName = activity.Author.UserName,
                                    activity.PublishTime,
                                    activity.SidForAuthor,
                                    activity.CoverImage,
                                    activity.Content,
                                    activity.Rating,
                                    activity.AttachedPoints,
                                    activity.Archived,
                                    activity.Rejected,
                                    PointId = activity.TargetPointId,
                                    PointIdCode = activity.TargetPoint.IdCode,
                                    PointAvatarImage = activity.TargetPoint.AvatarImage,
                                    PointChineseName = activity.TargetPoint.ChineseName,
                                    PointEnglishName = activity.TargetPoint.EnglishName
                                }).SingleOrDefaultAsync();
                            if (a == null || a.Archived != ArchivedState.None || (!ignoreRejected && a.Rejected))
                                continue;
                            card.Author = new UserBasicInfo
                            {
                                IdCode = a.AuthorIdCode,
                                AvatarImage = a.AuthorAvatarImage,
                                UserName = a.AuthorUserName,
                                IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                                    ? (bool?) null
                                    : await cachedData.Users.IsFriendAsync(currentUserId, a.AuthorId)
                            };
                            card.PublishTime = a.PublishTime;
                            card.ContentType = TimelineCardContentType.Activity;
                            card.ContentId = feed.Entry;
                            card.SidForAuthor = a.SidForAuthor;
                            card.CoverImage = a.CoverImage;
                            card.Content = a.Content;
                            card.Rating = a.Rating;
                            card.LikeCount =
                                await cachedData.Likes.GetTargetLikeCountAsync(feed.Entry, LikeTargetType.Activity);
                            card.Liked = string.IsNullOrWhiteSpace(currentUserId)
                                ? (bool?) null
                                : await
                                    cachedData.Likes.IsLikedAsync(currentUserId, feed.Entry, LikeTargetType.Activity);
                            card.CommentCount =
                                await cachedData.ActivityComments.GetActivityCommentCountAsync(feed.Entry);
                            targetPointId = a.PointId;
                            card.AttachedPoints.Add(new PointBasicInfo
                            {
                                IdCode = a.PointIdCode,
                                AvatarImage = a.PointAvatarImage,
                                ChineseName = a.PointChineseName,
                                EnglishName = a.PointEnglishName
                            });
                            break;
                        }

                        default:
                            continue;
                    }
                    var properties = Helpers.SafeDeserialize<SubscriptionStream.FeedProperties>(feed.Properties);
                    if (properties?.Reasons != null)
                    {
                        var splittedReasons = properties.Reasons.Select(r => r.Split(':')).ToList();
                        var likedByUserId = splittedReasons.Where(p => p.Length == 2 && p[0] == "like")
                            .Select(p => p[1]).FirstOrDefault();
                        if (likedByUserId != null)
                        {
                            var user = await dbContext.Users.Where(u => u.Id == likedByUserId).Select(u => new
                            {
                                u.IdCode,
                                u.AvatarImage,
                                u.UserName
                            }).SingleAsync();
                            card.LikedByUser = new UserBasicInfo
                            {
                                IdCode = user.IdCode,
                                AvatarImage = user.AvatarImage,
                                UserName = user.UserName
                            };
                        }
                        var pointIds = splittedReasons
                            .Where(p => p.Length == 2 && p[0] == "point" && p[1] != targetPointId)
                            .Select(p => p[1]).ToList();
                        if (pointIds.Count > 0)
                            card.AttachedPoints.AddRange((await (from point in dbContext.Points
                                where pointIds.Contains(point.Id)
                                select new
                                {
                                    point.IdCode,
                                    point.AvatarImage,
                                    point.ChineseName,
                                    point.EnglishName
                                }).ToListAsync()).Select(p => new PointBasicInfo
                                {
                                    IdCode = p.IdCode,
                                    AvatarImage = p.AvatarImage,
                                    ChineseName = p.ChineseName,
                                    EnglishName = p.EnglishName
                                }));
                    }
                    result.Add(card);
                }
                if (feeds.Count > 0) before = feeds.Last().Id;
            } while (feeds.Count >= take && result.Count < feeds.Count/2);
            return result;
        }

        /// <summary>
        /// 获取指定动态评论列表（用于轨道卡片）
        /// </summary>
        /// <param name="activityId">动态 ID</param>
        /// <param name="take">获取数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="before">起始位置</param>
        /// <returns><see cref="ActivityCommentList"/></returns>
        public static async Task<ActivityCommentList> GetComments(string activityId, int take,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, int before = int.MaxValue)
        {
            return await ActivityCommentList.CreateAsync(activityId, StateTreeHelper.GetCurrentUserId(),
                before, take, StateTreeHelper.GetCurrentUser().IsInRole(KeylolRoles.Operator), dbContext, cachedData);
        }

        /// <summary>
        /// 评论列表
        /// </summary>
        public ActivityCommentList Comments { get; set; }
    }

    /// <summary>
    /// 轨道卡片
    /// </summary>
    public class TimelineCard
    {
        /// <summary>
        /// Feed ID
        /// </summary>
        public int? FeedId { get; set; }

        /// <summary>
        /// 作者基本信息
        /// </summary>
        public UserBasicInfo Author { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 认可的用户基本信息
        /// </summary>
        public UserBasicInfo LikedByUser { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public TimelineCardContentType ContentType { get; set; }

        /// <summary>
        /// 内容 ID
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// 在作者名下的序号
        /// </summary>
        public int? SidForAuthor { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 投稿到的据点列表，第一项为主收稿据点
        /// </summary>
        public List<PointBasicInfo> AttachedPoints { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }
    }

    /// <summary>
    /// 时间轴卡片内容类型
    /// </summary>
    public enum TimelineCardContentType
    {
        /// <summary>
        /// 文章
        /// </summary>
        Article,

        /// <summary>
        /// 动态
        /// </summary>
        Activity
    }
}