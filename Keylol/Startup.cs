using System.Diagnostics;
using Keylol;
using Keylol.Utilities;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace Keylol
{
    public partial class Startup
    {
        private readonly EnableCorsRegexAttribute _corsPolicyProvider =
            new EnableCorsRegexAttribute(@"^(http|https)://([a-z-]+\.)?keylol\.com(:[0-9]{1,5})?/?$")
            {
                SupportsCredentials = true
            };

        public void Configuration(IAppBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var stopwatch = Stopwatch.StartNew();
                context.Response.OnSendingHeaders(o =>
                {
                    stopwatch.Stop();
                    context.Response.Headers.Set("X-Process-Time", stopwatch.ElapsedMilliseconds.ToString());
                }, null);
                await next.Invoke();
            });
            UseAuth(app);
            UseSignalR(app);
            UseWebAPI(app);
        }
    }
}