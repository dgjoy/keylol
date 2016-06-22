using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.PostOffice.SocialActivity
{
    /// <summary>
    /// 邮政中心 - 社交 - 听众
    /// </summary>
    public class SubscriberPage
    {
        /// <summary>
        /// 获取听众消息页
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SubscriberPage"/></returns>
        public static async Task<SubscriberPage> Get([Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext);
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<PostOfficeMessageList> GetMessages(int page, [Injected] KeylolDbContext dbContext)
        {
            return (await PostOfficeMessageList.CreateAsync(typeof(SubscriberPage), StateTreeHelper.GetCurrentUserId(),
                page, false, dbContext)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="SubscriberPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SubscriberPage"/></returns>
        public static async Task<SubscriberPage> CreateAsync(string currentUserId, KeylolDbContext dbContext)
        {
            var messages = await PostOfficeMessageList.CreateAsync(typeof(SubscriberPage), currentUserId, 1, true,
                dbContext);
            return new SubscriberPage
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