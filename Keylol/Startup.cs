using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Keylol.Startup))]
namespace Keylol
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
