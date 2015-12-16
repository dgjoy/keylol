using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Batch;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            config.EnableCors(_corsPolicyProvider);

#if DEBUG
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Keylol REST API")
                    .Contact(cc => cc.Name("Stackia")
                        .Email("stackia@keylol.com"));

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                c.IncludeXmlComments(Path.Combine(baseDirectory, "bin", "Keylol.XML"));
                c.DescribeAllEnumsAsStrings();
            }).EnableSwaggerUi();
#endif

            config.MapHttpAttributeRoutes();

            app.UseWebApi(server);
        }
    }
}