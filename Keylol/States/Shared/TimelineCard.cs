using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;

namespace Keylol.States.Shared
{
    /// <summary>
    /// 时间轴卡片列表
    /// </summary>
    public class TimelineCardList : List<TimelineCard>
    {
        private TimelineCardList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="TimelineCardList"/>
        /// </summary>
        /// <param name="streamName">Feed 流名称</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="take">获取数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="before">起始位置</param>
        /// <returns><see cref="TimelineCardList"/></returns>
        public static async Task<TimelineCardList> CreateAsync(string streamName, string currentUserId, int take,
            KeylolDbContext dbContext, CachedDataProvider cachedData, int before = int.MaxValue)
        {
            if (take > 50) take = 50;
            var feeds = await dbContext.Feeds.Where(f => f.StreamName == streamName && f.Id < before)
                .OrderByDescending(f => f.Id).Take(() => take).ToListAsync();
            var result = new TimelineCardList(feeds.Count);
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
                                PointId = article.TargetPointId,
                                PointIdCode = article.TargetPoint.IdCode,
                                PointAvatarImage = article.TargetPoint.AvatarImage,
                                PointChineseName = article.TargetPoint.ChineseName,
                                PointEnglishName = article.TargetPoint.EnglishName
                            }).SingleOrDefaultAsync();
                        if (a == null)
                            continue;
                        card.AuthorIdCode = a.AuthorIdCode;
                        card.AuthorAvatarImage = a.AuthorAvatarImage;
                        card.AuthorUserName = a.AuthorUserName;
                        card.AuthorIsFriend = string.IsNullOrWhiteSpace(currentUserId)
                            ? (bool?) null
                            : await cachedData.Users.IsFriendAsync(currentUserId, a.AuthorId);
                        card.PublishTime = a.PublishTime;
                        card.ContentType = TimelineCardContentType.Article;
                        card.ContentId = feed.Entry;
                        card.SidForAuthor = a.SidForAuthor;
                        card.CoverImage = a.CoverImage;
                        card.Title = a.Title;
                        card.Subtitle = a.Subtitle;
                        card.Rating = a.Rating;
                        card.AttachedPointCount = Helpers.SafeDeserialize<List<string>>(a.AttachedPoints)?.Count + 1 ??
                                                  1;
                        card.LikeCount =
                            await cachedData.Likes.GetTargetLikeCountAsync(feed.Entry, LikeTargetType.Article);
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
                                PointId = activity.TargetPointId,
                                PointIdCode = activity.TargetPoint.IdCode,
                                PointAvatarImage = activity.TargetPoint.AvatarImage,
                                PointChineseName = activity.TargetPoint.ChineseName,
                                PointEnglishName = activity.TargetPoint.EnglishName
                            }).SingleOrDefaultAsync();
                        if (a == null)
                            continue;
                        card.AuthorIdCode = a.AuthorIdCode;
                        card.AuthorAvatarImage = a.AuthorAvatarImage;
                        card.AuthorUserName = a.AuthorUserName;
                        card.AuthorIsFriend = string.IsNullOrWhiteSpace(currentUserId)
                            ? (bool?) null
                            : await cachedData.Users.IsFriendAsync(currentUserId, a.AuthorId);
                        card.PublishTime = a.PublishTime;
                        card.ContentType = TimelineCardContentType.Activity;
                        card.ContentId = feed.Entry;
                        card.SidForAuthor = a.SidForAuthor;
                        card.CoverImage = a.CoverImage;
                        card.Content = a.Content;
                        card.Rating = a.Rating;
                        card.AttachedPointCount = Helpers.SafeDeserialize<List<string>>(a.AttachedPoints)?.Count + 1 ??
                                                  1;
                        card.LikeCount =
                            await cachedData.Likes.GetTargetLikeCountAsync(feed.Entry, LikeTargetType.Activity);
                        card.CommentCount = await cachedData.ActivityComments.GetActivityCommentCountAsync(feed.Entry);
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
                var reasons = Helpers.SafeDeserialize<SubscriptionStream.FeedProperties>(feed.Properties);
                if (reasons?.Reasons != null)
                {
                    var pointIds = reasons.Reasons.Select(r => r.Split(':'))
                        .Where(p => p.Length == 2 && p[0] == "point" && p[1] != targetPointId)
                        .Select(p => p[1]);
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
            return result;
        }
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
        /// 作者是否是好友
        /// </summary>
        public bool? AuthorIsFriend { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        // TODO: 认可的人

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
        /// 投稿到的据点数
        /// </summary>
        public int? AttachedPointCount { get; set; }

        /// <summary>
        /// 投稿到的据点列表，第一项为主收稿据点
        /// </summary>
        public List<PointBasicInfo> AttachedPoints { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

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