using Keylol;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace Keylol
{
    public partial class Startup
    {
        private readonly EnableCorsRegexAttribute _corsPolicyProvider =
            new EnableCorsRegexAttribute(@"(http|https)://([a-z-]+\.)?keylol\.com") {SupportsCredentials = true};

        public void Configuration(IAppBuilder app)
        {
            UseAuth(app);
            UseSignalR(app);
            UseWebAPI(app);
        }
    }
}