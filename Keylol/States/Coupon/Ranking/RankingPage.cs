using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Coupon.Ranking
{
    /// <summary>
    /// 文券中心 - 排行
    /// </summary>
    public class RankingPage
    {
        /// <summary>
        /// 获取文券排行页
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="RankingPage"/></returns>
        public static async Task<RankingPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="RankingPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="RankingPage"/></returns>
        public static async Task<RankingPage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var rankingUsers = await RankingUserList.CreateAsync(currentUserId, 1, true, dbContext, cachedData);
            return new RankingPage
            {
                MyRanking = rankingUsers.Item2,
                RankingUsers = rankingUsers.Item1
            };
        }

        /// <summary>
        /// 我的排名
        /// </summary>
        public int? MyRanking { get; set; }

        /// <summary>
        /// 上榜用户列表
        /// </summary>
        public RankingUserList RankingUsers { get; set; }
    }
}