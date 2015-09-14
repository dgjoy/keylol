using System.Web.Http;
using System.Web.Http.Batch;

namespace Keylol
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpBatchRoute("batch", "api/batch", new DefaultHttpBatchHandler(GlobalConfiguration.DefaultServer));

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new {id = RouteParameter.Optional}
                );
        }
    }
}