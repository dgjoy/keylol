using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Services;

namespace Keylol.Controllers.SteamBot
{
    public partial class SteamBotController
    {
        /// <summary>
        /// 命令所有机器人给所有好友群发聊天消息
        /// </summary>
        /// <param name="message">消息内容</param>
        [Route("broadcast-message")]
        [HttpPost]
        public async Task<IHttpActionResult> BroadcaseMessage(string message)
        {
            foreach (var client in SteamBotCoordinator.Sessions.Values.Select(c => c.Client))
            {
                await client.BroadcastMessage(message);
            }
            return Ok();
        }
    }
}