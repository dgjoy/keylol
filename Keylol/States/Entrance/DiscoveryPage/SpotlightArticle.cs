using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Entrance.DiscoveryPage
{
    /// <summary>
    /// 精选文章列表
    /// </summary>
    public class SpotlightArticleList : List<SpotlightArticle>
    {
        private SpotlightArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="SpotlightArticleList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="spotlightArticleCategory">文章分类</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightArticleList"/></returns>
        public static async Task<SpotlightArticleList> CreateAsync(string currentUserId,
            SpotlightArticleStream.ArticleCategory spotlightArticleCategory, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var streamName = SpotlightArticleStream.Name(spotlightArticleCategory);
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == streamName
                join article in dbContext.Articles on feed.Entry equals article.Id
                orderby feed.Id descending
                select new
                {
                    article.AuthorId,
                    AuthorIdCode = article.Author.IdCode,
                    AuthorAvatarImage = article.Author.AvatarImage,
                    AuthorUserName = article.Author.UserName,
                    article.PublishTime,
                    article.SidForAuthor,
                    article.Title,
                    article.Subtitle,
                    article.CoverImage,
                    PointIdCode = article.TargetPoint.IdCode,
                    PointAvatarImage = article.TargetPoint.AvatarImage,
                    PointChineseName = article.TargetPoint.ChineseName,
                    PointEnglishName = article.TargetPoint.EnglishName,
                    PointSteamAppId = article.TargetPoint.SteamAppId
                })
                .Take(4)
                .ToListAsync();
            var result = new SpotlightArticleList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new SpotlightArticle
                {
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    AuthorIsFriend = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Users.IsFriend(currentUserId, a.AuthorId),
                    PublishTime = a.PublishTime,
                    SidForAuthor = a.SidForAuthor,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    CoverImage = a.CoverImage,
                    PointIdCode = a.PointIdCode,
                    PointAvatarImage = a.PointAvatarImage,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName,
                    PointInLibrary = string.IsNullOrWhiteSpace(currentUserId) || a.PointSteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibrary(currentUserId, a.PointSteamAppId.Value)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 精选文章
    /// </summary>
    public class SpotlightArticle
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
        /// 作者昵称
        /// </summary>
        public string AuthorUserName { get; set; }

        /// <summary>
        /// 作者是否是当前登录用户的好友
        /// </summary>
        public bool? AuthorIsFriend { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }

        /// <summary>
        /// 作者名下序号
        /// </summary>
        public int SidForAuthor { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 收稿据点识别码
        /// </summary>
        public string PointIdCode { get; set; }

        /// <summary>
        /// 收稿据点头像
        /// </summary>
        public string PointAvatarImage { get; set; }

        /// <summary>
        /// 收稿据点中文名
        /// </summary>
        public string PointChineseName { get; set; }

        /// <summary>
        /// 收稿据点英文名
        /// </summary>
        public string PointEnglishName { get; set; }

        /// <summary>
        /// 收稿据点是否已入库
        /// </summary>
        public bool? PointInLibrary { get; set; }
    }
}