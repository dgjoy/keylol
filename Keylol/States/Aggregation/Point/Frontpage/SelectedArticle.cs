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
    /// 文选文章列表
    /// </summary>
    public class SelectedArticleList : List<SelectedArticle>
    {
        private SelectedArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定据点的文选
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns></returns>
        public static async Task<SelectedArticleList> Get(string pointId, int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(pointId, page, 12, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="SelectedArticleList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SelectedArticleList"/></returns>
        public static async Task<SelectedArticleList> CreateAsync(string pointId, int page,
            int recordsPerPage, string currentUserId, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from article in dbContext.Articles
                where article.TargetPointId == pointId && article.Archived == ArchivedState.None &&
                      article.Rejected == false
                orderby dbContext.Likes
                    .Count(l => l.TargetId == article.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    article.Id,
                    article.Title,
                    article.Subtitle,
                    article.CoverImage,
                    article.SidForAuthor,
                    article.AuthorId,
                    AuthorIdCode = article.Author.IdCode,
                    AuthorAvatarImage = article.Author.AvatarImage,
                    AuthorUserName = article.Author.UserName
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new SelectedArticleList(queryResult.Count);
            foreach (var a in queryResult)
            {
                result.Add(new SelectedArticle
                {
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    CoverImage = a.CoverImage,
                    SidForAuthor = a.SidForAuthor,
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorAvatarImage = a.AuthorAvatarImage,
                    AuthorUserName = a.AuthorUserName,
                    AuthorIsFriend = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Users.IsFriendAsync(currentUserId, a.AuthorId),
                    LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Article),
                    CommentCount = await cachedData.ArticleComments.GetArticleCommentCountAsync(a.Id)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 文选文章
    /// </summary>
    public class SelectedArticle
    {
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
        /// 作者名下序号
        /// </summary>
        public int? SidForAuthor { get; set; }

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
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }
    }
}