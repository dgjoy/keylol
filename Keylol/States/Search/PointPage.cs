using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search
{
    /// <summary>
    /// 搜索据点页面
    /// </summary>
    public class PointPage
    {
        /// <summary>
        /// 搜索据点页面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<PointPage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="PointResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户Id</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<PointPage> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new PointPage
            {
                Results = await PointResultList.CreateAsync(currentUserId, keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 据点搜索列表
        /// </summary>
        public PointResultList Results { get; set; }
    }

    /// <summary>
    /// 据点搜索结果列表
    /// </summary>
    public class PointResultList : List<PointResult>
    {
        private PointResultList()
        {
        }

        private PointResultList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字搜索据点列表
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns><see cref="PointResultList"/></returns>
        public static async Task<PointResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="PointResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<PointResultList> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, int page,
            bool searchAll = true)
        {
            var onePageCount = searchAll ? 10 : 5;
            var offSet = (page - 1)*10;
            var searchResult = await dbContext.Database.SqlQuery<PointResult>(
                @"SELECT  *,
                          Id, 
                          IdCode,
                          (SELECT COUNT(1) FROM Articles WHERE TargetPointId = t4.Id) AS ArticleCount,
                          (SELECT COUNT(1) FROM Activities WHERE TargetPointId=t4.Id) AS ActivityCount,
                          AvatarImage 
                        FROM (SELECT * FROM [dbo].[Points] AS [t1] INNER JOIN (
                        SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
						SELECT * FROM CONTAINSTABLE([dbo].[Points], ([EnglishName], [EnglishAliases]), {0})
		                    UNION ALL
		                    SELECT * FROM CONTAINSTABLE([dbo].[Points], ([ChineseName], [ChineseAliases]), {0})
	                    ) AS [t2] GROUP BY [t2].[KEY]
                    ) AS [t3] ON [t1].[Id] = [t3].[KEY])AS [t4]
                    ORDER BY [t4].[RANK] DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", offSet,onePageCount).ToListAsync();
            var result = new PointResultList(searchResult.Count);
            if (searchAll)
            {
                foreach (var p in searchResult)
                {
                    result.Add(new PointResult
                    {
                        Id = p.Id,
                        IdCode = p.IdCode,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName,
                        AvatarImage = p.AvatarImage,
                        ReaderCount =
                            await
                                cachedData.Subscriptions.GetSubscriberCountAsync(p.Id, SubscriptionTargetType.User),
                        ArticleCount = p.ArticleCount,
                        ActivityCount = p.ActivityCount,
                        Subscribed =
                            await
                                cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                                    SubscriptionTargetType.User)
                    });
                }
            }
            else
            {
                foreach (var p in searchResult)
                {
                    result.Add(new PointResult
                    {
                        IdCode = p.IdCode,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName,
                        AvatarImage = p.AvatarImage
                    });
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 据点搜索结果
    /// </summary>
    public class PointResult
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 读者数
        /// </summary>
        public long? ReaderCount { get; set; }

        /// <summary>
        /// 来稿文章数
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态数
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 是否被订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}