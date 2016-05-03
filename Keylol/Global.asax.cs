using System;
using System.Web;
using Keylol.Provider;
using Keylol.ServiceBase;
using Keylol.ServiceBase.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SimpleInjector;
using SimpleInjector.Integration.Wcf;

namespace Keylol
{
    /// <summary>
    ///     ASP.NET 生命周期控制
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        ///     当应用启动时调用
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            // 此容器仅供 WCF 服务使用
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WcfOperationLifestyle();

            // log4net
            container.RegisterConditional(typeof(ILogProvider),
                c => typeof(LogProvider<>).MakeGenericType(c.Consumer?.ImplementationType ?? typeof(Startup)),
                Lifestyle.Singleton,
                c => true);

            // RabbitMQ IConnection
            container.RegisterSingleton<MqClientProvider>();

            // RabbitMQ IModel
            container.RegisterPerWcfOperation(() => container.GetInstance<MqClientProvider>().CreateModel());

            // StackExchange.Redis
            container.RegisterSingleton<RedisProvider>();

            // Transient Fault Handling Retry Policy
            container.RegisterSingleton<RetryPolicy>(() =>
            {
                // 首次失败立即重试，之后重试每次增加 2 秒间隔
                var strategy = new Incremental(3, TimeSpan.Zero, TimeSpan.FromSeconds(2));
                return new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(strategy);
            });

            SimpleInjectorServiceHostFactory.SetContainer(container);
        }
    }
}