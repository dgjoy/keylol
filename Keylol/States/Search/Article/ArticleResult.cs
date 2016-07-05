using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search.Article
{
    /// <summary>
    /// 文章搜索结果列表
    /// </summary>
    public class ArticleResultList : List<ArticleResult>
    {
        private ArticleResultList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字搜索文章列表
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否查询全部</param>
        public static async Task<ArticleResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            return await CreateAsync(keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="ArticleResultList"/>
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否查询全部</param>
        public static async Task<ArticleResultList> CreateAsync(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            var take = searchAll ? 10 : 5;
            var skip = (page - 1)*take;
            var searchResult = await dbContext.Database.SqlQuery<ArticleResult>(@"SELECT
                        *
                    FROM (SELECT
                        [t1].*,
                        [t2].*,
                        [t5].[IdCode] AS PointIdCode,
                        [t5].[AvatarImage] AS PointAvatarImage,
                        [t5].[ChineseName] AS PointChineseName,
                        [t5].[EnglishName] AS PointEnglishName,
                        [t6].[UserName] AS AuthorUserName,
                        [t6].[IdCode] AS AuthorIdCode,
                        [t6].[AvatarImage] AS AuthorAvatarImage
                    FROM [dbo].[Articles] AS [t1]
                    INNER JOIN (SELECT
                        *
                    FROM FREETEXTTABLE([dbo].[Articles], ([Title], [Subtitle], [UnstyledContent]), {0})) AS [t2]
                        ON [t1].[Sid] = [t2].[KEY]
                    INNER JOIN [dbo].[Points] AS [t5]
                        ON [t1].[TargetPointId] = [t5].[Id]
                    INNER JOIN [dbo].[KeylolUsers] AS [t6]
                        ON [t1].[AuthorId] = [t6].[Id]
                    WHERE [t1].[Archived] = 0 AND [t1].[Rejected] = 0) AS [t3]
                    ORDER BY [t3].[RANK] DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                keyword, skip, take).ToListAsync();

            var result = new ArticleResultList(searchResult.Count);
            foreach (var a in searchResult)
            {
                result.Add(new ArticleResult
                {
                    Title = a.Title,
                    SubTitle = a.SubTitle,
                    AuthorIdCode = a.AuthorIdCode,
                    AuthorUserName = searchAll ? a.AuthorUserName : null,
                    AuthorAvatarImage = searchAll ? a.AuthorAvatarImage : null,
                    SidForAuthor = a.SidForAuthor,
                    PointChineseName = searchAll ? a.PointChineseName : null,
                    PointEnglishName = searchAll ? a.PointEnglishName : null,
                    PointAvatarImage = searchAll ? a.PointAvatarImage : null,
                    PointIdCode = searchAll ? a.PointIdCode : null,
                    LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Article),
                    CommentCount =
                        searchAll ? await cachedData.ArticleComments.GetArticleCommentCountAsync(a.Id) : (long?)null,
                    PublishTime = searchAll ? a.PublishTime : null
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 文章搜索结果
    /// </summary>
    public class ArticleResult
    {
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
        public string SubTitle { get; set; }

        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 作者用户名
        /// </summary>
        public string AuthorUserName { get; set; }

        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorAvatarImage { get; set; }

        /// <summary>
        /// 文章在作者名下序号
        /// </summary>
        public int? SidForAuthor { get; set; }

        /// <summary>
        /// 据点中文名
        /// </summary>
        public string PointChineseName { get; set; }

        /// <summary>
        /// 据点英文名
        /// </summary>
        public string PointEnglishName { get; set; }

        /// <summary>
        /// 据点头像
        /// </summary>
        public string PointAvatarImage { get; set; }

        /// <summary>
        /// 据点识别码
        /// </summary>
        public string PointIdCode { get; set; }

        /// <summary>
        /// 获赞数
        /// </summary>
        public long? LikeCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public long? CommentCount { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }
    }
}