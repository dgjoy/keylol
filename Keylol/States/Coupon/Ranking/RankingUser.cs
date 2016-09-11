using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States.Coupon.Ranking
{
    /// <summary>
    /// 上榜用户列表
    /// </summary>
    public class RankingUserList : List<RankingUser>
    {
        private const int RecordsPerPage = 15;

        private RankingUserList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取文券上榜用户列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="RankingUserList"/></returns>
        public static async Task<RankingUserList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="RankingUserList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="RankingUserList"/></returns>
        public static async Task<RankingUserList> CreateAsync(string currentUserId, int page,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            if (page > 7) page = 7;
            var queryResult = await dbContext.Users.OrderByDescending(u => u.SeasonLikeCount)
                .ThenByDescending(u => u.Coupon)
                .Select(u => new
                {
                    u.Id,
                    u.IdCode,
                    u.UserName,
                    u.AvatarImage,
                    u.GamerTag,
                    u.SeasonLikeCount,
                    u.Coupon
                }).Take(100).ToListAsync();
            var actualResult = queryResult.Select((u, i) => new
            {
                Ranking = i,
                User = u
            }).Skip(RecordsPerPage*(page - 1)).Take(RecordsPerPage).ToList();
            var result = new RankingUserList(actualResult.Count);
            foreach (var g in actualResult)
            {
                result.Add(new RankingUser
                {
                    Ranking = g.Ranking,
                    UserBasicInfo = new UserBasicInfo
                    {
                        IdCode = g.User.IdCode,
                        AvatarImage = g.User.AvatarImage,
                        UserName = g.User.UserName,
                        GamerTag = g.User.GamerTag,
                        IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                            ? (bool?) null
                            : await cachedData.Users.IsFriendAsync(currentUserId, g.User.Id)
                    },
                    Coupon = g.User.Coupon,
                    SeasonLikeCount = g.User.SeasonLikeCount
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 上榜用户
    /// </summary>
    public class RankingUser
    {
        /// <summary>
        /// 排名
        /// </summary>
        public int? Ranking { get; set; }

        /// <summary>
        /// 用户基本信息
        /// </summary>
        public UserBasicInfo UserBasicInfo { get; set; }

        /// <summary>
        /// 文券数
        /// </summary>
        public int? Coupon { get; set; }

        /// <summary>
        /// 当季文章认可数
        /// </summary>
        public int? SeasonLikeCount { get; set; }
    }
}