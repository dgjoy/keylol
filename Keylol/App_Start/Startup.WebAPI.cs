using System.Web.Http;
using System.Web.Http.Batch;
using Owin;

namespace Keylol
{
    public partial class Startup
    {
        public void UseWebAPI(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var server = new HttpServer(config);

            config.EnableCors(_corsPolicyProvider);

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpBatchRoute("Batch", "batch",
                new DefaultHttpBatchHandler(server));

            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new {id = RouteParameter.Optional});

            app.UseWebApi(server);
        }
    }
}