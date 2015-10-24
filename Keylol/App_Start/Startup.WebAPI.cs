using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Batch;
using Newtonsoft.Json;
using Owin;
using Swashbuckle.Application;

namespace Keylol
{
    public partial class Startup
    {
        public void UseWebAPI(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var server = new HttpServer(config);

            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            config.EnableCors(_corsPolicyProvider);

            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Keylol REST API");

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                c.IncludeXmlComments(Path.Combine(baseDirectory, "bin", "Keylol.XML"));
            }).EnableSwaggerUi();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpBatchRoute("Batch", "batch",
                new DefaultHttpBatchHandler(server));

            app.UseWebApi(server);
        }
    }
}