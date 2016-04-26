using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Services;
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
        /// <param name="tempSilence">是否在两分钟内关闭机器人的自动回复（图灵机器人），对于 userId 为 "*" 的情况无效，默认 false</param>
        /// <param name="idType">ID 类型，默认 "IdCode"</param>
        [Route("message")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> SendMessage(string userId, string message, bool tempSilence = false,
            UserIdentityType idType = UserIdentityType.IdCode)
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
                    case UserIdentityType.UserName:
                        user = await _userManager.FindByNameAsync(userId);
                        break;

                    case UserIdentityType.IdCode:
                        user = await _userManager.FindByIdCodeAsync(userId);
                        break;

                    case UserIdentityType.Id:
                        user = await _userManager.FindByIdAsync(userId);
                        break;

                    case UserIdentityType.SteamId:
                        user = await _userManager.FindBySteamIdAsync(userId);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
                }

                if (user == null)
                    return NotFound();

                await _userManager.SendSteamChatMessageAsync(user, message, tempSilence);
            }
            return Ok();
        }
    }
}