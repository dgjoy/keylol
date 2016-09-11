using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.PostOffice
{
    /// <summary>
    /// 邮政中心 - 未读
    /// </summary>
    public class UnreadPage
    {
        /// <summary>
        /// 获取未读消息页
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="UnreadPage"/></returns>
        public static async Task<UnreadPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取未读消息列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PostOfficeMessageList"/></returns>
        public static async Task<PostOfficeMessageList> GetMessages(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return (await PostOfficeMessageList.CreateAsync(typeof(UnreadPage), StateTreeHelper.GetCurrentUserId(),
                page, false, dbContext, cachedData)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="UnreadPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="UnreadPage"/></returns>
        public static async Task<UnreadPage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var messages = await PostOfficeMessageList.CreateAsync(typeof(UnreadPage), currentUserId, 1, true,
                dbContext, cachedData);
            return new UnreadPage
            {
                MessagePageCount = messages.Item2,
                Messages = messages.Item1
            };
        }

        /// <summary>
        /// 消息总页数
        /// </summary>
        public int? MessagePageCount { get; set; }

        /// <summary>
        /// 消息列表
        /// </summary>
        public PostOfficeMessageList Messages { get; set; }
    }
}