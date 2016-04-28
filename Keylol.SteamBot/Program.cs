using System.ServiceModel;
using ChannelAdam.ServiceModel;
using Keylol.ServiceBase;
using Keylol.ServiceBase.TransientFaultHandling;
using Keylol.SteamBot.ServiceReference;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace Keylol.SteamBot
{
    public static class Program
    {
        public static Container Container { get; } = new Container();

        public static void Main(string[] args)
        {
            // 服务特定依赖注册点
            Container.RegisterSingleton<SteamBotCoordinatorCallback>();
            Container.RegisterSingleton(() => ServiceConsumerFactory.Create<ISteamBotCoordinator>(
                () =>
                    new SteamBotCoordinatorClient(
                        new InstanceContext(Container.GetInstance<SteamBotCoordinatorCallback>()))
                    {
                        ClientCredentials =
                        {
                            UserName =
                            {
                                UserName = "keylol-service-consumer",
                                Password = "neLFDyJB8Vj2Xtsn2KMTUEFw"
                            }
                        }
                    }, new RetryPolicyAdapter(Container.GetInstance<RetryPolicy>()),
                new NullServiceConsumerExceptionBehaviourStrategy()));

            Lifestyle.Transient.CreateRegistration<BotCookieManager>(Container)
                .SuppressDisposableTransientComponentWarning();
            Lifestyle.Transient.CreateRegistration<BotInstance>(Container).SuppressDisposableTransientComponentWarning();

            KeylolService.Run<SteamBot>(args, Container);
        }

        private static void SuppressDisposableTransientComponentWarning(this Registration registration)
        {
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                "Dispose is ensured to be manually called.");
        }
    }
}