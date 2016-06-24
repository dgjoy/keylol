using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Services;
using Keylol.Utilities;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供 Steam 绑定服务
    /// </summary>
    public class SteamBindingHub : Hub<ISteamBindingHubClient>
    {
        private static int _nextBot;

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /></returns>
        public override async Task OnConnected()
        {
            using (var dbContext = new KeylolDbContext())
            {
                var bot = await GetNextBotAsync(dbContext);
                var random = new Random();
                string code;
                do
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < 4; i++)
                    {
                        sb.Append((char) random.Next('A', 'Z'));
                    }
                    sb.Append(random.Next(0, 10000).ToString("D4"));
                    code = sb.ToString();
                } while (dbContext.SteamBindingTokens.Any(t => t.Code == code));

                var token = new SteamBindingToken
                {
                    Code = code,
                    BrowserConnectionId = Context.ConnectionId,
                    BotId = bot?.Id
                };
                dbContext.SteamBindingTokens.Add(token);
                await dbContext.SaveChangesAsync();

                Clients.Caller.OnCode(token.Id, code, bot?.SteamId, $"其乐机器人 #{bot?.Sid}");
            }
        }

        /// <summary>
        /// 获取下一个可用的机器人
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SteamBot"/></returns>
        public static async Task<SteamBot> GetNextBotAsync(KeylolDbContext dbContext)
        {
            var sessions = SteamBotCoordinator.Sessions.Keys;
            var bots = await dbContext.SteamBots.Where(b =>
                b.Online && sessions.Contains(b.SessionId)
                && b.SteamId != null && b.FriendCount < b.FriendUpperLimit && b.Enabled)
                .OrderBy(b => b.Sid)
                .ToListAsync();

            if (bots.Count == 0) return null;
            if (_nextBot >= bots.Count)
                _nextBot = 0;
            return bots[_nextBot++];
        }

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
                var tokens = await dbContext.SteamBindingTokens
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
            }
        }
    }

    /// <summary>
    /// <see cref="SteamBindingHub"/> Client
    /// </summary>
    public interface ISteamBindingHubClient
    {
        /// <summary>
        /// 通知新的绑定验证码
        /// </summary>
        /// <param name="tokenId">Steam Binding Token ID</param>
        /// <param name="code">绑定验证码</param>
        /// <param name="botSteamId">机器人 Steam ID</param>
        /// <param name="botName">机器人名称</param>
        void OnCode(string tokenId, string code, string botSteamId, string botName);

        /// <summary>
        /// 通知已经接受了好友请求
        /// </summary>
        void OnFriend();

        /// <summary>
        /// 通知已经成功收到了绑定验证码
        /// </summary>
        /// <param name="steamProfileName">Steam 资料昵称</param>
        /// <param name="steamAvatarHash">Steam 头像 Hash</param>
        void OnBind(string steamProfileName, string steamAvatarHash);
    }
}