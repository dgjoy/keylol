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
        /// <returns></returns>
        public static async Task<PointPage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId,keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="PointResultList"/>
        /// </summary>
        /// <param name="userId">当前用户Id</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<PointPage> CreateAsync(string userId,string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new PointPage { 
                Results = await PointResultList.CreateAsync(userId,keyword, dbContext,cachedData,1,searchAll)
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
        /// <param name="userId">当前用户Id</param>
        /// <param name="keyword">关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData">缓存数据</param>
        /// <param name="page">页码</param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns><see cref="PointResultList"/></returns>
        public static async Task<PointResultList> Get(string keyword,[Injected] KeylolDbContext dbContext,
            [Injected]CachedDataProvider cachedData,int page, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="PointResultList"/>
        /// </summary>
        /// <param name="userId">当前用户Id</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">页码</param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<PointResultList> CreateAsync(string userId,string keyword, [Injected] KeylolDbContext dbContext,
            [Injected]CachedDataProvider cachedData,int page,bool searchAll = true)
        {
            var offSet = (page - 1)*10;
            var searchResult = await dbContext.Database.SqlQuery<PointResult>(
                @"SELECT  *,
                          Id AS PointId, 
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
                    ORDER BY [t4].[RANK] DESC OFFSET {1} ROWS FETCH NEXT 10 ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", offSet).ToListAsync();
            var result = new PointResultList(searchResult.Count);
            if (searchAll)
            {
                foreach (var p in searchResult)
                {
                    result.Add(new PointResult
                    {
                        PointId = p.PointId,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName,
                        AvatarImage = p.AvatarImage,
                        ReaderCount = await cachedData.Subscriptions.GetSubscriberCountAsync(p.PointId, SubscriptionTargetType.User),
                        ArticleCount = p.ArticleCount,
                        ActivityCount = p.ActivityCount,
                        IsSubscribed = await cachedData.Subscriptions.IsSubscribedAsync(userId,p.PointId, SubscriptionTargetType.User)
                    });
                }
            }
            else
            {
                foreach (var p in searchResult)
                {
                    result.Add(new PointResult
                    {
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
        /// 据点ID
        /// </summary>
        public string PointId { get; set; }
        /// <summary>
        /// 据点中文名称
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 据点英文名称
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 据点头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 读者数量
        /// </summary>
        public long? ReaderCount { get; set; }

        /// <summary>
        /// 来稿文章数量
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态数量
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 是否被订阅
        /// </summary>
        public bool IsSubscribed { get; set; }

    }
}
