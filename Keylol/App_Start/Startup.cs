using System;
using System.Diagnostics;
using System.IO;
using System.Web.Http;
using Keylol;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
        private readonly EnableCorsRegexAttribute _corsPolicyProvider =
            new EnableCorsRegexAttribute(@"^(http|https)://([a-z-]+\.)?keylol\.com(:[0-9]{1,5})?/?$")
            {
                SupportsCredentials = true
            };

        /// <summary>
        /// 全局 IoC 容器
        /// </summary>
        public static Container Container { get; set; } = new Container();

        /// <summary>
        ///     OWIN 配置
        /// </summary>
        public void Configuration(IAppBuilder app)
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
            Container.Register(() => Container.GetInstance<MqClientProvider>().CreateModel(), Lifestyle.Scoped);

            // StackExchange.Redis
            Container.RegisterSingleton<RedisProvider>();

            // Keylol DbContext
            Container.Register<KeylolDbContext>(Lifestyle.Scoped);

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

            // 认证相关中间件
            UseAuth(app);

            // SignalR
            UseSignalR(app);

            // ASP.NET Web API
            UseWebApi(app);

            Container.Verify();
        }

        private void UseSignalR(IAppBuilder app)
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

        private void UseWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var server = new HttpServer(config);

            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            config.EnableCors(_corsPolicyProvider);
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