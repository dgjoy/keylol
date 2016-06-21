using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.PostOffice
{
    /// <summary>
    /// 邮政中心 - 公函
    /// </summary>
    public class MissivePage
    {
        private const int RecordsPerPage = 10;

        /// <summary>
        /// 获取公函消息列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="MissivePage"/></returns>
        public static async Task<MissivePage> Get(int page, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="MissivePage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="MissivePage"/></returns>
        public static async Task<MissivePage> CreateAsync(string currentUserId, int page, KeylolDbContext dbContext)
        {
            var count = await dbContext.Messages.CountAsync(m => m.ReceiverId == currentUserId &&
                                                                 (int) m.Type >= 200 && (int) m.Type <= 299);
            return new MissivePage
            {
                MessagePageCount = count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1,
                Messages = PostOfficeMessageList.Create(await dbContext.Messages.IncludeRelated()
                    .Where(m => m.ReceiverId == currentUserId &&
                                (int) m.Type >= 200 && (int) m.Type <= 299)
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