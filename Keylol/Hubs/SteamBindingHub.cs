using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services;
using Keylol.Utilities;
using Microsoft.AspNet.SignalR;
using SteamKit2;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供 Steam 绑定服务
    /// </summary>
    public class SteamBindingHub : Hub<ISteamBindingHubClient>
    {
        private static int _botSkip;

        /// <summary>
        /// Called when a connection disconnects from this hub gracefully or due to a timeout.
        /// </summary>
        /// <param name="stopCalled">
        /// true, if stop was called on the client closing the connection gracefully;
        /// false, if the connection has been lost for longer than the
        /// <see cref="P:Microsoft.AspNet.SignalR.Configuration.IConfigurationManager.DisconnectTimeout" />.
        /// Timeouts can be caused by clients reconnecting to another SignalR server in scaleout.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /></returns>
        public override async Task OnDisconnected(bool stopCalled)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var tokens =
                    await dbContext.SteamBindingTokens
                        .Where(t => t.BrowserConnectionId == Context.ConnectionId)
                        .ToListAsync();
                foreach (var token in tokens)
                {
                    if (token.SteamId != null)
                    {
                        if (token.Bot.IsOnline())
                        {
                            var botCoordinator = SteamBotCoordinator.Sessions[token.Bot.SessionId];
                            await botCoordinator.Client.RemoveFriend(token.BotId, token.SteamId);
                        }
                    }
                    dbContext.SteamBindingTokens.Remove(token);
                }
                await dbContext.SaveChangesAsync();
                await base.OnDisconnected(stopCalled);
            }
        }

        /// <summary>
        /// 创建一个新的 Steam Binding Token
        /// </summary>
        /// <returns></returns>
        public async Task<SteamBindingTokenDto> CreateToken()
        {
            using (var dbContext = new KeylolDbContext())
            {
                var sessions = SteamBotCoordinator.Sessions.Keys;
                var bot = await dbContext.SteamBots.Where(b =>
                    b.Online && sessions.Contains(b.SessionId)
                    && b.SteamId != null && b.FriendCount < b.FriendUpperLimit && b.Enabled)
                    .OrderBy(b => b.SequenceNumber)
                    .Skip(() => _botSkip)
                    .FirstOrDefaultAsync();
                if (_botSkip >= await dbContext.SteamBots.CountAsync() - 1)
                    _botSkip = 0;
                else
                    _botSkip++;
                if (bot == null)
                    return null;

                var random = new Random();
                string code;
                do
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < 4; i++)
                    {
                        sb.Append((char) random.Next('A', 'Z'));
                    }
                    for (var i = 0; i < 4; i++)
                    {
                        sb.Append(random.Next(0, 10));
                    }
                    code = sb.ToString();
                } while (await dbContext.SteamBindingTokens.AnyAsync(t => t.Code == code));

                var token = new SteamBindingToken
                {
                    Code = code,
                    BrowserConnectionId = Context.ConnectionId
                };
                dbContext.SteamBindingTokens.Add(token);
                await dbContext.SaveChangesAsync();

                var steamId = new SteamID();
                steamId.SetFromSteam3String(bot.SteamId);
                return new SteamBindingTokenDto
                {
                    Id = token.Id,
                    Code = token.Code,
                    BotSteamId64 = steamId.ConvertToUInt64().ToString()
                };
            }
        }
    }

    /// <summary>
    /// <see cref="SteamBindingHub"/> Client
    /// </summary>
    public interface ISteamBindingHubClient
    {
        /// <summary>
        /// 通知已经接受了好友请求
        /// </summary>
        void NotifySteamFriendAdded();

        /// <summary>
        /// 通知已经成功收到了绑定验证码
        /// </summary>
        /// <param name="steamProfileName">Steam 资料昵称</param>
        /// <param name="steamAvatarHash">Steam 头像 Hash</param>
        void NotifyCodeReceived(string steamProfileName, string steamAvatarHash);
    }
}