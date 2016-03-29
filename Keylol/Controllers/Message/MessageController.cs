using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Message
{
    /// <summary>
    ///     消息 Controller（邮政中心）
    /// </summary>
    [Authorize]
    [RoutePrefix("message")]
    public partial class MessageController : KeylolApiController
    {
        /// <summary>
        /// 创建 MessageController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public MessageController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}