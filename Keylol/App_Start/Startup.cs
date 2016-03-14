using System.Diagnostics;
using Keylol;
using Keylol.Hubs;
using Keylol.Models.DAL;
using Keylol.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using WebApiThrottle;

[assembly: OwinStartup(typeof (Startup))]

namespace Keylol
{
    /// <summary>
    ///     OWIN 入口
    /// </summary>
    public partial class Startup
    {
        private readonly EnableCorsRegexAttribute _corsPolicyProvider =
            new EnableCorsRegexAttribute(@"^(http|https)://([a-z-]+\.)?keylol\.com(:[0-9]{1,5})?/?$")
            {
                SupportsCredentials = true
            };

        /// <summary>
        ///     OWIN 配置
        /// </summary>
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            var debugInfoHub = GlobalHost.ConnectionManager.GetHubContext<DebugInfoHub, IDebugInfoHubClient>();
            KeylolDbContext.LogAction = s =>
            {
                debugInfoHub.Clients.All.Write(s);
                Debug.Write(s);
            };
#endif

            // 请求处理计时中间件
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

            // 访问频率限制中间件
            app.Use(typeof (ThrottlingMiddleware),
                ThrottlePolicy.FromStore(new PolicyConfigurationProvider()),
                new PolicyMemoryCacheRepository(),
                new MemoryCacheRepository(),
                null, null);
            UseAuth(app);
            UseSignalR(app);
            UseWebApi(app);
        }
    }
}