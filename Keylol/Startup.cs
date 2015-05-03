using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Keylol.Startup))]
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
