using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using Keylol;
using Keylol.Hubs;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.ServiceBase;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.WebApi;
using Swashbuckle.Application;
using WebApiThrottle;

[assembly: OwinStartup(typeof (Startup))]

namespace Keylol
{
    /// <summary>
    ///     OWIN 入口
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 全局 IoC 容器
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
                    context.Response.Headers.Set("X-Process-Time", stopwatch.ElapsedMilliseconds.ToString());
                }, null);
                await next.Invoke();
            });

            // 为后续 OWIN 中间件建立 IoC 容器 Scope
            app.Use(async (context, next) =>
            {
                using (Container.BeginExecutionContextScope())
                    await next.Invoke();
            });

            // 访问频率限制
            app.Use(typeof (ThrottlingMiddleware),
                ThrottlePolicy.FromStore(new PolicyConfigurationProvider()),
                new PolicyMemoryCacheRepository(),
                new MemoryCacheRepository(),
                null, null);

            // CORS
            UseCors(app);

            // 认证、授权相关中间件
            UseAuth(app);

            // SignalR
            app.MapSignalR();

            // ASP.NET Web API
            UseWebApi(app);

            Container.Verify();
        }

        private void RegisterServices()
        {
            // 配置容器
            Container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            // log4net
            Container.RegisterConditional(typeof (ILogProvider),
                c => typeof (LogProvider<>).MakeGenericType(c.Consumer?.ImplementationType ?? typeof (Startup)),
                Lifestyle.Singleton,
                c => true);

            // RabbitMQ IConnection
            Container.RegisterSingleton<MqClientProvider>();

            // RabbitMQ IModel
            Container.RegisterWebApiRequest(() => Container.GetInstance<MqClientProvider>().CreateModel());

            // StackExchange.Redis
            Container.RegisterSingleton<RedisProvider>();

            // Keylol DbContext
            Container.Register(() =>
            {
                var context = new KeylolDbContext();
#if DEBUG
                context.WriteLog +=
                    (sender, s) => { GlobalHost.ConnectionManager.GetHubContext<DebugInfoHub>().Clients.All.Write(s); };
#endif
                return context;
            }, Lifestyle.Scoped);

            // Coupon
            Container.RegisterWebApiRequest<CouponProvider>();

            // Statistics
            Container.RegisterWebApiRequest<StatisticsProvider>();
        }

        private void UseCors(IAppBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // 将所有 'X-' Headers 加入到 Access-Control-Expose-Headers 中
                context.Response.OnSendingHeaders(o =>
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers",
                        context.Response.Headers.Select(h => h.Key)
                            .Where(k => k.StartsWith("X-", StringComparison.OrdinalIgnoreCase)).ToArray());
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

        private void UseWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var server = new HttpServer(config);

            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

#if DEBUG
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

            Container.RegisterWebApiControllers(config);
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(Container);

            app.UseWebApi(server);
        }

        private void UseAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext<KeylolDbContext>((o, c) => Container.GetInstance<KeylolDbContext>(),
                (o, c) => { }); // 忽略 disposeCallback，交给 Container 自身处理
            app.CreatePerOwinContext<KeylolUserManager>(KeylolUserManager.Create);
            app.CreatePerOwinContext<KeylolSignInManager>(KeylolSignInManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = PathString.Empty,
                CookieHttpOnly = true,
                CookieName = ".Keylol.Cookies",
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromDays(15),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity =
                        SecurityStampValidator.OnValidateIdentity<KeylolUserManager, KeylolUser>(
                            TimeSpan.FromMinutes(30), (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

//            app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions()
//            {
//                AllowInsecureHttp = true,
//                TokenEndpointPath = new PathString("/oauth/token"),
//                AuthorizeEndpointPath = new PathString("/oauth/authorize"),
//                Provider = new KeylolOAuthProvider()
//            });
        }
    }
}