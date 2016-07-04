
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search
{
    /// <summary>
    /// 文章搜索
    /// </summary>
    public class ArticlePage
    {
        /// <summary>
        /// 搜索文章界面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<ArticlePage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return await CreateAsync(keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建<see cref="ArticleResultList"/>
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="dbContext"></param>
        /// <param name="cachedData"></param>
        /// <param name="searchAll"></param>
        /// <returns></returns>
        public static async Task<ArticlePage> CreateAsync(string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData,bool searchAll = true)
        {
            return new ArticlePage
            {
                Results = await ArticleResultList.CreateAsync(keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 文章搜索结果
        /// </summary>
        public ArticleResultList Results { get; set; }
    }

    /// <summary>
    /// 文章搜索结果列表
    /// </summary>
    public class ArticleResultList : List<ArticleResult>
    {
        private ArticleResultList()
        {
        }

        private ArticleResultList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字搜索文章列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="dbContext"></param>
        /// <param name="cachedData"></param>
        /// <param name="page"></param>
        /// <param name="searchAll"></param>
        /// <returns></returns>
        public static async Task<ArticleResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            return await CreateAsync(keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="ArticleResultList"/>
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="dbContext"></param>
        /// <param name="cachedData"></param>
        /// <param name="page"></param>
        /// <param name="searchAll"></param>
        /// <returns></returns>
        public static async Task<ArticleResultList> CreateAsync(string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            int onePageCount;
            if (searchAll)
            {
                onePageCount = 10;
            }
            else
            {
                onePageCount = 5;
            }
            var offSet = (page - 1)*10;
            var searchResult = await dbContext.Database.SqlQuery<ArticleResult>(
                @"SELECT *,
                         (SELECT AvatarImage FROM Points WHERE t4.TargetPointId= Id)AS AvatarImage,
                         (SELECT IdCode FROM KeylolUsers WHERE t4.AuthorId = Id)AS AuthorIdCode,
                         (SELECT ChineseName FROM Points WHERE t4.TargetPointId = Id)AS TargetPointChineseName,
					     (SELECT EnglishName FROM Points WHERE t4.TargetPointId = Id)AS TargetPointEnglishName
                    FROM(
					    SELECT * FROM [dbo].[Articles] AS [t1] INNER JOIN
					        (SELECT [t2].[KEY],SUM([t2].[RANK]) AS RANK FROM (
					            SELECT * FROM CONTAINSTABLE([dbo].[Articles],([Title],[Subtitle],[Content]),{0})
                            ) AS[t2] GROUP BY[t2].[KEY]) AS[t3] ON[t1].[Id] = [t3].[KEY]) AS[t4]
                    ORDER BY[t4].RANK DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY", 
                $"\"{keyword}\" OR \"{keyword}*\"", offSet,onePageCount).ToListAsync();
            var result = new ArticleResultList(searchResult.Count);
            if (searchAll)
            {
                foreach (var p in searchResult)
                {
                    result.Add(new ArticleResult
                    {
                        Titile = p.Titile,
                        SubTitle = p.SubTitle,
                        AutherUserIdCode = p.AutherUserIdCode,
                        SidForAuther = p.SidForAuther,
                        TargetPointChineseName = p.TargetPointChineseName,
                        TargetPointEnglishName = p.TargetPointEnglishName,
                        TargetPointAvater = p.TargetPointAvater,
                        LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(p.Id, LikeTargetType.Article),
                        CommentCount = await cachedData.ArticleComments.GetArticleCommentCountAsync(p.Id),
                        PublishTime = p.PublishTime
                    });
                }
            }
            else
            {
                foreach (var p in searchResult)
                {
                    result.Add(new ArticleResult
                    {
                        Titile = p.Titile,
                        SubTitle = p.SubTitle,
                        AutherUserIdCode = p.AutherUserIdCode,
                        SidForAuther = p.SidForAuther,
                        TargetPointChineseName = p.TargetPointChineseName,
                        TargetPointEnglishName = p.TargetPointEnglishName,
                        TargetPointAvater = p.TargetPointAvater,
                        LikeCount = await cachedData.Likes.GetTargetLikeCountAsync(p.Id, LikeTargetType.Article)
                    });
                }
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
        /// 文章ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Titile { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 作者用户识别码
        /// </summary>
        public string AutherUserIdCode { get; set; }

        /// <summary>
        /// 作者的文章定位
        /// </summary>
        public int SidForAuther { get; set; }

        /// <summary>
        /// 投稿据点ID
        /// </summary>
        public string TargetPointId { get; set; }

        /// <summary>
        /// 投稿据点中文
        /// </summary>
        public string TargetPointChineseName { get; set; }

        /// <summary>
        /// 投稿据点英文
        /// </summary>
        public string TargetPointEnglishName { get; set; }

        /// <summary>
        /// 据点头像
        /// </summary>
        public string TargetPointAvater { get; set; }

        /// <summary>
        /// 获赞数
        /// </summary>
        public long LikeCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public long CommentCount { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }
    }
}