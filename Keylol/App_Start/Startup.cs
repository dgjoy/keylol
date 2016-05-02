using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.Controllers;
using Keylol.Filters;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using Swashbuckle.Application;
using WebApiThrottle;
using SimpleInjector.Integration.Owin;

namespace Keylol
{
    /// <summary>
    ///     OWIN 入口
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     全局 IoC 容器
        /// </summary>
        public static Container Container { get; set; } = new Container();

        /// <summary>
        ///     OWIN 启动配置
        /// </summary>
        public void Configuration(IAppBuilder app)
        {
            // 注册常用服务
            RegisterServices();

            // 启用 OWIN 中间件

            // 请求计时
            app.Use(async (context, next) =>
            {
                var stopwatch = Stopwatch.StartNew();
                context.Response.OnSendingHeaders(o =>
                {
                    stopwatch.Stop();
                    try
                    {
                        context.Response.Headers.Set("X-Process-Time", stopwatch.ElapsedMilliseconds.ToString());
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }, null);
                await next.Invoke();
            });

            // 错误页面
            app.UseErrorPage(ErrorPageOptions.ShowAll);

            // 访问频率限制
            app.Use(typeof (ThrottlingMiddleware),
                ThrottlePolicy.FromStore(new PolicyConfigurationProvider()),
                new PolicyMemoryCacheRepository(),
                new MemoryCacheRepository(),
                null, null);

            // CORS
            UseCors(app);
            
            // Simple Injector OWIN Request Lifestyle
            app.UseOwinRequestLifestyle();

            // OAuth 认证服务器
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(7),
                AuthenticationMode = AuthenticationMode.Passive,
                TokenEndpointPath = new PathString("/oauth/token"),
                AuthorizeEndpointPath = new PathString("/oauth/authorization"),
                Provider = new KeylolOAuthAuthorizationServerProvider()
            });
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new KeylolOAuthBearerAuthenticationProvider()
            });

            app.UseStageMarker(PipelineStage.Authenticate);

            // SignalR
            app.MapSignalR();

            // ASP.NET Web API
            UseWebApi(app);

            Container.Verify();
        }

        private static void RegisterServices()
        {
            Container.Options.DefaultScopedLifestyle = new OwinRequestLifestyle();

            // log4net
            Container.RegisterConditional(typeof (ILogProvider),
                c => typeof (LogProvider<>).MakeGenericType(c.Consumer?.ImplementationType ?? typeof (Startup)),
                Lifestyle.Singleton,
                c => true);

            // RabbitMQ IConnection
            Container.RegisterSingleton<MqClientProvider>();

            // RabbitMQ IModel
            Container.RegisterPerOwinRequest(() => Container.GetInstance<MqClientProvider>().CreateModel());

            // StackExchange.Redis
            Container.RegisterSingleton<RedisProvider>();

            // Geetest
            Container.RegisterSingleton<GeetestProvider>();

            // OWIN Context Provider
            Container.RegisterSingleton<OwinContextProvider>();

            // Keylol DbContext
            Container.RegisterPerOwinRequest(() =>
            {
                var context = new KeylolDbContext();
#if DEBUG
                context.WriteLog +=
                    (sender, s) => { GlobalHost.ConnectionManager.GetHubContext<DebugInfoHub>().Clients.All.Write(s); };
#endif
                return context;
            });

            // Keylol User Manager
            Container.RegisterPerOwinRequest<KeylolUserManager>();

            // Coupon
            Container.RegisterPerOwinRequest<CouponProvider>();

            // Statistics
            Container.RegisterPerOwinRequest<StatisticsProvider>();

            // One-time Token
            Container.RegisterSingleton<OneTimeTokenProvider>();
        }

        private static void UseCors(IAppBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // 将所有 'X-' Headers 加入到 Access-Control-Expose-Headers 中
                context.Response.OnSendingHeaders(o =>
                {
                    try
                    {
                        context.Response.Headers.Add("Access-Control-Expose-Headers",
                            context.Response.Headers.Select(h => h.Key)
                                .Where(k => k.StartsWith("X-", StringComparison.OrdinalIgnoreCase)).ToArray());
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }, null);
                await next.Invoke();
            });
            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = request =>
                    {
                        if (!Regex.IsMatch(request.Headers["Origin"],
                            @"^(http|https)://([a-z-]+\.)?keylol\.com(:[0-9]{1,5})?/?$", RegexOptions.IgnoreCase))
                            return Task.FromResult<CorsPolicy>(null);
                        return Task.FromResult(new CorsPolicy
                        {
                            AllowAnyHeader = true,
                            AllowAnyOrigin = true,
                            AllowAnyMethod = true,
                            SupportsCredentials = true,
                            PreflightMaxAge = 365*24*3600
                        });
                    }
                }
            });
        }

        private static void UseWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var server = new HttpServer(config);

            var jsonSerializerSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());

            config.Services.Replace(typeof (IActionValueBinder), new KeylolActionValueProvider());
            config.Filters.Add(new ValidateModelAttribute());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.EnableSwagger("swagger-hRwp3Pnm/docs/{apiVersion}", c =>
            {
                c.SingleApiVersion("v2", "Keylol API")
                    .Description("The API specification for Keylol backend.")
                    .Contact(cc => cc.Name("Stackia")
                        .Email("stackia@keylol.com")
                        .Url("http://t.cn/RqWeGJf"));

                c.Schemes(new[] {"https"});

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                c.IncludeXmlComments(Path.Combine(baseDirectory, "bin", "Keylol.XML"));
                var dtoXml = Path.Combine(baseDirectory, "bin", "Keylol.Models.DTO.XML");
                if (File.Exists(dtoXml))
                    c.IncludeXmlComments(dtoXml);
            }).EnableSwaggerUi("swagger-hRwp3Pnm/{*assetPath}",
                c => { c.InjectJavaScript(Assembly.GetExecutingAssembly(), "Keylol.Utilities.swagger-ui-extra.js"); });

            config.MapHttpAttributeRoutes();

            Container.RegisterWebApiControllers(config);
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(Container);

            app.UseWebApi(server);
        }
    }
}