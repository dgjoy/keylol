using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.PostOffice.SocialActivity
{
    /// <summary>
    /// 邮政中心 - 社交 - 评论
    /// </summary>
    public class CommentPage
    {
        /// <summary>
        /// 获取评论消息页
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="CommentPage"/></returns>
        public static async Task<CommentPage> Get([Injected] KeylolDbContext dbContext)
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
            return (await PostOfficeMessageList.CreateAsync(typeof(CommentPage), StateTreeHelper.GetCurrentUserId(),
                page, false, dbContext)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="CommentPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="CommentPage"/></returns>
        public static async Task<CommentPage> CreateAsync(string currentUserId, KeylolDbContext dbContext)
        {
            var messages = await PostOfficeMessageList.CreateAsync(typeof(CommentPage), currentUserId, 1, true,
                dbContext);
            return new CommentPage
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