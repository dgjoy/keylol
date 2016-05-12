using System;
using System.Threading.Tasks;
using Keylol.Provider;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供通过 Steam 登录服务
    /// </summary>
    public class SteamLoginHub : Hub<ISteamLoginHubClient>
    {
        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /></returns>
        public override async Task OnConnected()
        {
            var oneTimeTokenProvider = Global.Container.GetInstance<OneTimeTokenProvider>();
            var random = new Random();
            var code = await oneTimeTokenProvider.Generate(Context.ConnectionId,
                TimeSpan.FromMinutes(10), OneTimeTokenPurpose.SteamLogin,
                () => Task.FromResult(random.Next(0, 10000).ToString("D4")));
            Clients.Caller.OnCode(code);
        }
    }

    /// <summary>
    /// <see cref="SteamLoginHub"/> Client
    /// </summary>
    public interface ISteamLoginHubClient
    {
        /// <summary>
        /// 通知新的登录验证码
        /// </summary>
        /// <param name="code">登录验证码</param>
        void OnCode(string code);

        /// <summary>
        /// 通知新的登录用 One-time Token
        /// </summary>
        /// <param name="token">登录用 One-time Token</param>
        void OnLoginOneTimeToken(string token);
    }
}