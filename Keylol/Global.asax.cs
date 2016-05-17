using System;
using System.Web;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;
using Keylol.ServiceBase.TransientFaultHandling;
using Keylol.Utilities;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SimpleInjector;
using SimpleInjector.Integration.Owin;
using SimpleInjector.Integration.Wcf;
using SimpleInjector.Integration.WebApi;

namespace Keylol
{
    /// <summary>
    ///     ASP.NET 生命周期控制
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        ///     全局 IoC 容器
        /// </summary>
        public static Container Container { get; private set; }

        /// <summary>
        ///     当应用启动时调用（优先级高于 Startup 类）
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            Container = new Container();
            RegisterServices();
            SimpleInjectorServiceHostFactory.SetContainer(Container);
        }

        /// <summary>
        /// 当应用程序结束时调用
        /// </summary>
        protected void Application_End(object sender, EventArgs e)
        {
            Container.Dispose();
        }

        private static void RegisterServices()
        {
            Container.Options.DefaultScopedLifestyle = new OwinRequestLifestyle();

            // log4net
            SetupLogger();
            Container.RegisterConditional(typeof(ILogProvider),
                c => typeof(LogProvider<>).MakeGenericType(c.Consumer?.ImplementationType ?? typeof(Startup)),
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
//                context.Database.Log = s => { NotificationProvider.Hub<LogHub, ILogHubClient>().All.OnWrite(s); };
#endif
                return context;
            });

            // Keylol User Manager
            Container.RegisterPerOwinRequest<KeylolUserManager>();

            // Keylol Role Manager
            Container.RegisterPerOwinRequest<KeylolRoleManager>();

            // Coupon
            Container.RegisterPerOwinRequest<CouponProvider>();

            // Statistics
            Container.RegisterPerOwinRequest<CachedDataProvider>();

            // One-time Token
            Container.RegisterSingleton<OneTimeTokenProvider>();

            // Transient Fault Handling Retry Policy
            Container.RegisterSingleton<RetryPolicy>(() =>
            {
                // 首次失败立即重试，之后重试每次增加 2 秒间隔
                var strategy = new Incremental(3, TimeSpan.Zero, TimeSpan.FromSeconds(2));
                return new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(strategy);
            });

            // Steam Crawler
            Container.RegisterPerOwinRequest<SteamCrawlerProvider>();

            // HttpConfiguration / Web API Controllers
            var httpConfiguration = new HttpConfiguration();
            Container.RegisterWebApiControllers(httpConfiguration);
            httpConfiguration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(Container);
            Container.RegisterSingleton(() => httpConfiguration);

            Container.Verify();
        }

        private static void SetupLogger()
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();
            var appender = new LogHubAppender();
            appender.ActivateOptions();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }
    }
}