using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Keylol.ServiceBase.TransientFaultHandling;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SimpleInjector;

namespace Keylol.ServiceBase
{
    /// <summary>
    ///     所有微服务都要继承于这个类
    /// </summary>
    public class KeylolService : System.ServiceProcess.ServiceBase
    {
        /// <summary>
        ///     服务中止事件
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        ///     为容器注册公用依赖（log4net 和 RabbitMQ Iconnection），然后启动新服务
        /// </summary>
        /// <param name="args">服务启动参数</param>
        /// <param name="container">IoC 容器</param>
        /// <typeparam name="TService">要启动的服务类型</typeparam>
        public static void Run<TService>(string[] args, Container container) where TService : KeylolService
        {
            // 公用服务注册点

            // log4net
            container.RegisterConditional(typeof (ILogProvider),
                c => typeof (LogProvider<>).MakeGenericType(c.Consumer?.ImplementationType ?? typeof (KeylolService)),
                Lifestyle.Singleton,
                c => true);

            // RabbitMQ IConnection
            container.RegisterSingleton<MqClientProvider>();

            // Transient Fault Handling Retry Policy
            container.RegisterSingleton<RetryPolicy>(() =>
            {
                // 首次失败立即重试，之后重试每次增加 2 秒间隔
                var strategy = new Incremental(6, TimeSpan.Zero, TimeSpan.FromSeconds(2));
                return new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(strategy);
            });

            // 自身也注册进入容器
            container.RegisterSingleton<KeylolService, TService>();

            container.Verify();

            var service = container.GetInstance<KeylolService>();

            if (service.UseSelfInstaller(args))
            {
                container.Dispose();
                return;
            }

            SetupLogger(service.EventLog.Source);
            service.Stopped += (sender, eventArgs) => container.Dispose();

            if (Environment.UserInteractive) // 作为控制台应用启动
            {
                Console.Title =
                    $"Service Console: {(string.IsNullOrWhiteSpace(service.ServiceName) ? "(unnamed)" : service.ServiceName)}";
                Console.WriteLine("Running in console mode. Press Ctrl-Q to stop.");
                service.OnStart(args);
                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                        break;
                }
                service.OnStop();
            }
            else // 作为 Windows 服务启动
            {
                Run(service);
            }
        }

        /// <summary>
        ///     继承后需要确保调用本方法，以触发 Stopped 事件
        /// </summary>
        protected override void OnStop()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private static void SetupLogger(string eventSource)
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            if (Environment.UserInteractive)
            {
                var coloredConsoleAppender = new ColoredConsoleAppender
                {
                    Layout = patternLayout
                };
                coloredConsoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Debug,
                    ForeColor = ColoredConsoleAppender.Colors.Green
                });
                coloredConsoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Info,
                    ForeColor = ColoredConsoleAppender.Colors.White
                });
                coloredConsoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Warn,
                    ForeColor = ColoredConsoleAppender.Colors.Yellow
                });
                coloredConsoleAppender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Error,
                    ForeColor = ColoredConsoleAppender.Colors.White,
                    BackColor = ColoredConsoleAppender.Colors.Red
                });
                coloredConsoleAppender.ActivateOptions();
                hierarchy.Root.AddAppender(coloredConsoleAppender);
            }
            else
            {
                var eventLogAppender = new EventLogAppender
                {
                    ApplicationName = eventSource,
                    Layout = patternLayout
                };
                eventLogAppender.ActivateOptions();
                hierarchy.Root.AddAppender(eventLogAppender);
            }

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }

        private bool UseSelfInstaller(string[] args)
        {
            if (!Environment.UserInteractive) return false;
            if (args.Contains("--install", StringComparer.OrdinalIgnoreCase))
            {
                ManagedInstallerClass.InstallHelper(new[]
                {"/LogFile=", Assembly.GetEntryAssembly().Location});

                // 自动设定恢复选项为无限重启
                int exitCode;
                using (var process = new Process())
                {
                    var startInfo = process.StartInfo;
                    startInfo.FileName = "sc";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    startInfo.Arguments = $"failure \"{ServiceName}\" reset= 0 actions= restart/60000";

                    process.Start();
                    process.WaitForExit();

                    exitCode = process.ExitCode;
                }

                if (exitCode != 0)
                    throw new InvalidOperationException("Cannot set recovery options automatically.");
            }
            else if (args.Contains("--uninstall", StringComparer.OrdinalIgnoreCase))
            {
                ManagedInstallerClass.InstallHelper(new[]
                {"/LogFile=", "/u", Assembly.GetEntryAssembly().Location});
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}