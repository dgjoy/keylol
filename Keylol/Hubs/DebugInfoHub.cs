#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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