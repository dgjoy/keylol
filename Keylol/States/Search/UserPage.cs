
using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search
{
    /// <summary>
    /// 用户搜索页面
    /// </summary>
    public class UserPage
    {
        /// <summary>
        /// 搜索用户页面返回结果
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部搜索</param>
        public static async Task<UserPage> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, dbContext, cachedData, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="UserPage"/>
        /// </summary>
        /// <param name="currentUserId">当前用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<UserPage> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, bool searchAll = true)
        {
            return new UserPage
            {
                Results = await UserResultList.CreateAsync(currentUserId, keyword, dbContext, cachedData, 1, searchAll)
            };
        }

        /// <summary>
        /// 用户搜索列表
        /// </summary>
        public UserResultList Results { get; set; }
    }

    /// <summary>
    /// 用户搜索列表
    /// </summary>
    public class UserResultList : List<UserResult>
    {
        private UserResultList()
        {
        }

        private UserResultList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字搜索用户列表
        /// </summary>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<UserResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="UserResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns></returns>
        public static async Task<UserResultList> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, int page,
            bool searchAll = true)
        {
            var onePageCount = searchAll ? 10 : 5;
            var offSet = (page - 1) * 10;
            var searchResult = await dbContext.Database.SqlQuery<UserResult>(
                @"SELECT *,
                        (SELECT UserName)AS Name,
						(SELECT COUNT(1) FROM Articles WHERE AuthorId = t4.Id)AS ArticleCount,
						(SELECT COUNT(1) FROM Activities WHERE AuthorId = t4.Id)AS ActivityCount 
					FROM 
					(SELECT * FROM [dbo].[KeylolUsers] AS [t1] INNER JOIN
					(SELECT [t2].[KEY],SUM([t2].[Rank]) AS RANK FROM(
					SELECT * FROM CONTAINSTABLE([dbo].[KeylolUsers],([UserName]),{0})
                    ) AS[t2] GROUP BY[t2].[KEY]) AS[t3] ON[t1].[Sid] = [t3].[KEY]) AS[t4]
                    ORDER BY[t4].RANK DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", offSet, onePageCount).ToListAsync();
            var result = new UserResultList(searchResult.Count);
            if (searchAll)
            {
                foreach (var p in searchResult)
                {
                    result.Add(new UserResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        GamerTag = p.GamerTag,
                        IdCode = p.IdCode,
                        AvatarImage = p.AvatarImage,
                        ArticleCount = p.ArticleCount,
                        ActivityCount = p.ActivityCount,
                        Like = await cachedData.Likes.GetUserLikeCountAsync(p.Id),
                        IsFriend = await cachedData.Users.IsFriendAsync(currentUserId,p.Id)
                    });
                }
            }
            else
            {
                foreach (var p in searchResult)
                {
                    result.Add(new UserResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        GamerTag = p.GamerTag,
                        IdCode = p.IdCode,
                        AvatarImage = p.AvatarImage,
                        ArticleCount = p.ArticleCount,
                        ActivityCount = p.ActivityCount,
                        Like = await cachedData.Likes.GetUserLikeCountAsync(p.Id),
                        IsFriend = await cachedData.Users.IsFriendAsync(currentUserId, p.Id)
                    });
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 用户搜索结果
    /// </summary>
    public class UserResult
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用户识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 座右铭
        /// </summary>
        public string GamerTag { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 文章数量
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 获得认可数量
        /// </summary>
        public int? Like { get; set; }

        /// <summary>
        /// 互为好友
        /// </summary>
        public bool? IsFriend { get; set; }
    }
}