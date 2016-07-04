using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search.User
{
    /// <summary>
    /// 用户搜索列表
    /// </summary>
    public class UserResultList : List<UserResult>
    {
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
        public static async Task<UserResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
        {
            return
                await CreateAsync(StateTreeHelper.GetCurrentUserId(), keyword, dbContext, cachedData, page, searchAll);
        }

        /// <summary>
        /// 创建 <see cref="UserResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="page">分页页码</param>
        /// <param name="searchAll">是否全部查询</param>
        public static async Task<UserResultList> CreateAsync(string currentUserId, string keyword,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData, int page,
            bool searchAll = true)
        {
            var take = searchAll ? 10 : 5;
            var skip = (page - 1)*take;
            keyword = keyword.Replace('"', ' ').Replace('*', ' ').Replace('\'', ' ');
            var queryResult = await dbContext.Database.SqlQuery<UserResult>(@"SELECT
                        *,
                        (SELECT
                            COUNT(1)
                        FROM Articles
                        WHERE AuthorId = [t3].[Id])
                        AS ArticleCount,
                        (SELECT
                            COUNT(1)
                        FROM Activities
                        WHERE AuthorId = [t3].[Id])
                        AS ActivityCount
                    FROM (SELECT
                        *
                    FROM [dbo].[KeylolUsers] AS [t1]
                    INNER JOIN (SELECT
                        *
                    FROM CONTAINSTABLE([dbo].[KeylolUsers], ([UserName]), {0})) AS [t2]
                        ON [t1].[Sid] = [t2].[KEY]) AS [t3]
                    ORDER BY [t3].[RANK] DESC, [ArticleCount] DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync();

            var result = new UserResultList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new UserResult
                {
                    Id = searchAll ? p.Id : null,
                    UserName = p.UserName,
                    GamerTag = p.GamerTag,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ArticleCount = p.ArticleCount,
                    ActivityCount = p.ActivityCount,
                    LikeCount = await cachedData.Likes.GetUserLikeCountAsync(p.Id),
                    IsFriend = await cachedData.Users.IsFriendAsync(currentUserId, p.Id)
                });
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
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 玩家标签
        /// </summary>
        public string GamerTag { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 文章数
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态数
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 获得认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否是好友
        /// </summary>
        public bool? IsFriend { get; set; }
    }
}