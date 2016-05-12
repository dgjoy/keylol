using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// Spotlight Article List
    /// </summary>
    public class SpotlightArticleList : List<SpotlightArticle>
    {
        private SpotlightArticleList([NotNull] IEnumerable<SpotlightArticle> collection) : base(collection)
        {
        }

        /// <summary>
        /// 创建 <see cref="SpotlightArticleList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="spotlightArticleCategory">文章分类</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SpotlightArticleList"/></returns>
        public static async Task<SpotlightArticleList> CreateAsync(string currentUserId,
            SpotlightArticleStream.ArticleCategory spotlightArticleCategory, KeylolDbContext dbContext)
        {
            var streamName = SpotlightArticleStream.Name(spotlightArticleCategory);
            return new SpotlightArticleList((await (from feed in dbContext.Feeds
                where feed.StreamName == streamName
                join article in dbContext.Articles on feed.Entry equals article.Id
                orderby feed.Id descending
                select new
                {
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
                    PointEnglishName = article.TargetPoint.EnglishName
                })
                .Take(4)
                .ToListAsync())
                .Select(a => new SpotlightArticle
                {
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    AuthorIsFriend = true, // TODO
                    PublishTime = a.PublishTime,
                    SidForAuthor = a.SidForAuthor,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    CoverImage = a.CoverImage,
                    PointIdCode = a.PointIdCode,
                    PointAvatarImage = a.PointAvatarImage,
                    PointChineseName = a.PointChineseName,
                    PointEnglishName = a.PointEnglishName,
                    PointInLibrary = a.PointIdCode == null ? (bool?) null : true // TODO
                }));
        }
    }

    /// <summary>
    /// Spotlight Article
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
        /// 文章在作者名下的序号
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