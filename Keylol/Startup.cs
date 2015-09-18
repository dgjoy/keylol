using System;
using Keylol;
using Keylol.DAL;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace Keylol
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureWebAPI(app);
            app.MapSignalR(new HubConfiguration {EnableJavaScriptProxies = false});
        }
    }
}