using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
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
        /// <param name="tempSilence">是否在两分钟内关闭机器人的自动回复（图灵机器人），对于 userId 为 "*" 的情况无效，默认 false</param>
        /// <param name="idType">ID 类型，默认 "IdCode"</param>
        [Route("message")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> SendMessage(string userId, string message, bool tempSilence = false,
            UserController.IdType idType = UserController.IdType.IdCode)
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

                    case UserController.IdType.SteamId:
                        user = await DbContext.Users.SingleOrDefaultAsync(u => u.SteamId == userId);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
                }

                if (user == null)
                    return NotFound();

                if (user.SteamBot.IsOnline())
                {
                    if (tempSilence)
                    {
                        if (SteamBotCoordinator.AutoChatDisabledBots.ContainsKey(user.SteamBotId))
                        {
                            var timer = SteamBotCoordinator.AutoChatDisabledBots[user.SteamBotId];
                            timer.Stop();
                            timer.Start();
                        }
                        else
                        {
                            var timer = new Timer(120000) {AutoReset = false};
                            timer.Elapsed +=
                                (sender, args) =>
                                {
                                    SteamBotCoordinator.AutoChatDisabledBots.TryRemove(user.SteamBotId, out timer);
                                };
                            timer.Start();
                            SteamBotCoordinator.AutoChatDisabledBots[user.SteamBotId] = timer;
                        }
                    }
                    else if (SteamBotCoordinator.AutoChatDisabledBots.ContainsKey(user.SteamBotId))
                    {
                        Timer timer;
                        if (SteamBotCoordinator.AutoChatDisabledBots.TryRemove(user.SteamBotId, out timer))
                        {
                            timer.Stop();
                        }
                    }
                    await SteamBotCoordinator.Sessions[user.SteamBot.SessionId]
                        .Client.SendChatMessage(user.SteamBotId, user.SteamId, message, true);
                }
            }
            return Ok();
        }
    }
}