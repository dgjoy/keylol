using Keylol.Identity;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供实时日志输出服务
    /// </summary>
    [Authorize(Roles = KeylolRoles.Operator)]
    public class LogHub : Hub<ILogHubClient>
    {
    }

    /// <summary>
    /// <see cref="LogHub"/> Client
    /// </summary>
    public interface ILogHubClient
    {
        /// <summary>
        /// 通知输出一条日志信息
        /// </summary>
        /// <param name="message">消息内容</param>
        void OnWrite(string message);
    }
}