using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.PostOffice.SocialActivity
{
    /// <summary>
    /// 邮政中心 - 社交 - 听众
    /// </summary>
    public class SubscriberPage
    {
        private const int RecordsPerPage = 10;

        /// <summary>
        /// 获取听众消息列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SubscriberPage"/></returns>
        public static async Task<SubscriberPage> Get(int page, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="SubscriberPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SubscriberPage"/></returns>
        public static async Task<SubscriberPage> CreateAsync(string currentUserId, int page, KeylolDbContext dbContext)
        {
            var count = await dbContext.Messages.CountAsync(m => m.ReceiverId == currentUserId &&
                                                                 (int) m.Type >= 100 && (int) m.Type <= 199);
            return new SubscriberPage
            {
                MessagePageCount = count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1,
                Messages = PostOfficeMessageList.Create(await dbContext.Messages.IncludeRelated()
                    .Where(m => m.ReceiverId == currentUserId &&
                                (int) m.Type >= 300 && (int) m.Type <= 399)
                    .OrderByDescending(m => m.Unread)
                    .ThenByDescending(m => m.Sid)
                    .TakePage(page, RecordsPerPage)
                    .ToListAsync())
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