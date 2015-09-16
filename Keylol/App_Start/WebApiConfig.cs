using System.Web.Http;
using System.Web.Http.Batch;
using System.Web.Http.Cors;

namespace Keylol
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            config.EnableCors(new EnableCorsRegexAttribute(@"(http|https)://([a-z-]+\.)?keylol\.com")
            {
                SupportsCredentials = true
            });

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpBatchRoute("batch", "batch",
                new DefaultHttpBatchHandler(GlobalConfiguration.DefaultServer));

            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new {id = RouteParameter.Optional}
                );
        }
    }
}