#if DEBUG
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    public interface IDebugInfoHubClient
    {
        void WriteLine(string message);
        void Write(string message);
    }

    public class DebugInfoHub : Hub<IDebugInfoHubClient>
    {
    }
}

#endif