using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Controllers.User;
using Keylol.Models;
using Keylol.Services;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.SteamBot
{
    public partial class SteamBotController
    {
        /// <summary>
        /// 命令机器人给指定用户发送聊天消息
        /// </summary>
        /// <param name="userId">用户 ID，"*" 表示所有机器人给所有好友发送消息</param>
        /// <param name="message">消息内容</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("message")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> SendMessage(string userId, string message,
            UserController.IdType idType = UserController.IdType.Id)
        {
            if (userId == "*")
            {
                foreach (var client in SteamBotCoordinator.Sessions.Values.Select(c => c.Client))
                {
                    await client.BroadcastMessage(message);
                }
            }
            else
            {
                KeylolUser user;
                switch (idType)
                {
                    case UserController.IdType.UserName:
                        user = await DbContext.Users.SingleOrDefaultAsync(u => u.UserName == userId);
                        break;

                    case UserController.IdType.IdCode:
                        user = await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == userId);
                        break;

                    case UserController.IdType.Id:
                        user = await DbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
                }

                if (user == null)
                    return NotFound();

                if (user.SteamBot.IsOnline())
                {
                    await SteamBotCoordinator.Sessions[user.SteamBot.SessionId]
                        .Client.SendChatMessage(user.SteamBotId, user.SteamId, message, true);
                }
            }
            return Ok();
        }
    }
}