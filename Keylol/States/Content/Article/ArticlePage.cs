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
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States.Content.Article
{
    /// <summary>
    /// 内容 - 文章
    /// </summary>
    public class ArticlePage
    {
        /// <summary>
        /// 获取一篇文章
        /// </summary>
        /// <param name="authorIdCode">作者识别码</param>
        /// <param name="sidForAuthor">文章在作者名下的序号</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ArticlePage"/></returns>
        public static async Task<ArticlePage> Get(string authorIdCode, int sidForAuthor,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(authorIdCode, sidForAuthor, StateTreeHelper.GetCurrentUserId(),
                StateTreeHelper.GetCurrentUser().IsInRole(KeylolRoles.Operator), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="ArticlePage"/>
        /// </summary>
        /// <param name="authorIdCode">作者识别码</param>
        /// <param name="sidForAuthor">文章在作者名下的序号</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="isOperator">当前登录用户是否为运维职员</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ArticlePage"/></returns>
        public static async Task<ArticlePage> CreateAsync(string authorIdCode, int sidForAuthor, string currentUserId,
            bool isOperator, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var articlePage = new ArticlePage();

            var article = await dbContext.Articles
                .Include(a => a.Author)
                .Include(a => a.TargetPoint)
                .Where(a => a.Author.IdCode == authorIdCode && a.SidForAuthor == sidForAuthor)
                .SingleOrDefaultAsync();

            if (article == null)
                return articlePage;

            articlePage.Archived = article.Archived != ArchivedState.None;
            if (articlePage.Archived.Value && currentUserId != article.AuthorId && !isOperator)
                return articlePage;

            articlePage.PointBasicInfo =
                await PointBasicInfo.CreateAsync(currentUserId, article.TargetPoint, dbContext, cachedData);
            articlePage.AuthorBasicInfo = new UserBasicInfo
            {
                Id = article.Author.Id,
                IdCode = article.Author.IdCode,
                AvatarImage = article.Author.AvatarImage,
                UserName = article.Author.UserName,
                FriendCount = await cachedData.Subscriptions.GetFriendCountAsync(article.AuthorId),
                SubscribedUserCount = await cachedData.Subscriptions
                    .GetSubscribedUserCountAsync(article.AuthorId),
                SubscriberCount = await cachedData.Subscriptions
                    .GetSubscriberCountAsync(article.AuthorId, SubscriptionTargetType.User),
                SteamProfileName = article.Author.SteamProfileName,
                IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Users.IsFriendAsync(currentUserId, article.AuthorId),
                Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                    ? (bool?) null
                    : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, article.AuthorId,
                        SubscriptionTargetType.User)
            };
            articlePage.AuthorPlayedTime = article.TargetPoint.SteamAppId == null
                ? null
                : (await dbContext.UserSteamGameRecords
                    .Where(r => r.UserId == article.AuthorId && r.SteamAppId == article.TargetPoint.SteamAppId)
                    .SingleOrDefaultAsync())?.TotalPlayedTime;
            articlePage.Id = article.Id;
            articlePage.Title = article.Title;
            articlePage.Subtitle = article.Subtitle;
            var attachedPointIds = Helpers.SafeDeserialize<List<string>>(article.AttachedPoints) ?? new List<string>();
            articlePage.AttachedPoints = (from id in attachedPointIds
                join point in await (from point in dbContext.Points
                    where attachedPointIds.Contains(point.Id)
                    select new
                    {
                        point.Type,
                        point.Id,
                        point.IdCode,
                        point.AvatarImage,
                        point.ChineseName,
                        point.EnglishName
                    }).ToListAsync() on id equals point.Id
                select new PointBasicInfo
                {
                    Type = point.Type,
                    Id = point.Id,
                    IdCode = point.IdCode,
                    AvatarImage = point.AvatarImage,
                    ChineseName = point.ChineseName,
                    EnglishName = point.EnglishName
                }).ToList();
            articlePage.PublishTime = article.PublishTime;
            articlePage.Rejected = article.Rejected;
            articlePage.Spotlighted = article.Spotlighted;
            articlePage.Warned = article.Warned;
            articlePage.Content = article.Content;
            articlePage.ReproductionRequirement =
                Helpers.SafeDeserialize<ReproductionRequirement>(article.ReproductionRequirement);
            articlePage.LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(article.Id, LikeTargetType.Article);
            articlePage.Liked = string.IsNullOrWhiteSpace(currentUserId)
                ? (bool?) null
                : await cachedData.Likes.IsLikedAsync(currentUserId, article.Id, LikeTargetType.Article);
            articlePage.Rating = article.Rating;
            articlePage.CoverImage = article.CoverImage;
            articlePage.Pros = Helpers.SafeDeserialize<List<string>>(article.Pros);
            articlePage.Cons = Helpers.SafeDeserialize<List<string>>(article.Cons);
            articlePage.RecommendedArticles =
                await RecommendedArticleList.CreateAsync(article.Id, article.AuthorId, article.TargetPointId, dbContext);
            var comments = await ArticleCommentList.CreateAsync(article, 1, currentUserId, isOperator, true, dbContext,
                cachedData);
            articlePage.CommentCount = comments.Item2;
            articlePage.LatestCommentTime = comments.Item3;
            articlePage.CommentPageCount = comments.Item4;
            articlePage.Comments = comments.Item1;
            return articlePage;
        }

        /// <summary>
        /// 据点基本信息
        /// </summary>
        public PointBasicInfo PointBasicInfo { get; set; }

        /// <summary>
        /// 作者基本信息
        /// </summary>
        public UserBasicInfo AuthorBasicInfo { get; set; }

        /// <summary>
        /// 作者在档时间
        /// </summary>
        public double? AuthorPlayedTime { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 关联投稿据点列表
        /// </summary>
        public List<PointBasicInfo> AttachedPoints { get; set; }

        /// <summary>
        /// 是否被封存
        /// </summary>
        public bool? Archived { get; set; }

        /// <summary>
        /// 是否被退稿
        /// </summary>
        public bool? Rejected { get; set; }

        /// <summary>
        /// 是否被萃选
        /// </summary>
        public bool? Spotlighted { get; set; }

        /// <summary>
        /// 是否被警告
        /// </summary>
        public bool? Warned { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 转载要求
        /// </summary>
        public ReproductionRequirement ReproductionRequirement { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 优点
        /// </summary>
        public List<string> Pros { get; set; }

        /// <summary>
        /// 缺点
        /// </summary>
        public List<string> Cons { get; set; }

        /// <summary>
        /// 推荐文章
        /// </summary>
        public RecommendedArticleList RecommendedArticles { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 最新评论时间
        /// </summary>
        public DateTime? LatestCommentTime { get; set; }

        /// <summary>
        /// 评论总页数
        /// </summary>
        public int? CommentPageCount { get; set; }

        /// <summary>
        /// 评论列表
        /// </summary>
        public ArticleCommentList Comments { get; set; }
    }
}