using Microsoft.Owin.Cors;
using Owin;

namespace Keylol
{
    public partial class Startup
    {
        public void UseSignalR(IAppBuilder app)
        {
            app.Map("/signalr", a =>
            {
                a.UseCors(new CorsOptions
                {
                    PolicyProvider = _corsPolicyProvider
                });
                a.RunSignalR();
            });
        }
    }
}