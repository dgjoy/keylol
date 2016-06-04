using System;
using System.Collections.Generic;
using System.Configuration;
using log4net;
using RabbitMQ.Client;

namespace Keylol.ServiceBase
{
    /// <summary>
    ///     RabbitMQ Client 提供者
    /// </summary>
    public class MqClientProvider : IDisposable
    {
        private readonly ILog _logger;
        private bool _disposed;

        /// <summary>
        ///     创建新 MqClientProvider
        /// </summary>
        public MqClientProvider(ILogProvider log)
        {
            _logger = log.Logger;
            Connection = new ConnectionFactory
            {
                Uri = ConfigurationManager.AppSettings["rabbitMqConnection"] ?? "amqp://localhost/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true
            }.CreateConnection();
            Connection.ConnectionShutdown += OnConnectionShutdown;
            ((IRecoverable) Connection).Recovery +=
                (sender, eventArgs) => { _logger.Info("RabbitMQ connection recovered."); };

            // 统一声明交换机、队列
            using (var channel = CreateModel())
            {
                channel.ExchangeDeclare(DelayedMessageExchange, "x-delayed-message", true, false,
                    new Dictionary<string, object> {{"x-delayed-type", "direct"}});
                channel.QueueDeclare(ImageGarageRequestQueue, true, false, false, null);
                channel.QueueDeclare(PushHubRequestQueue, true, false, false, null);
            }
        }

        /// <summary>
        ///     RabbitMQ Client IConnection 对象
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        ///     资源清理
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _logger.Warn(
                $"RabbitMQ connection shutdown.{(shutdownEventArgs.Cause == null ? string.Empty : $" Reason: {shutdownEventArgs.Cause}")}");
        }

        /// <summary>
        ///     创建新频道
        /// </summary>
        /// <returns>创建的 IModel 对象</returns>
        public IModel CreateModel() => Connection.CreateModel();

        /// <summary>
        ///     资源清理
        /// </summary>
        /// <param name="disposing">是否清理托管对象</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Connection.ConnectionShutdown -= OnConnectionShutdown;
                Connection.Dispose();
                _logger.Info("RabbitMQ Connection closed.");
            }
            _disposed = true;
        }

        #region 预定义名称

        /// <summary>
        ///     延迟消息交换机
        /// </summary>
        public static readonly string DelayedMessageExchange = "delayed-message-exchange";

        /// <summary>
        ///     Image Garage 请求队列
        /// </summary>
        public static readonly string ImageGarageRequestQueue = "image-garage-requests";

        /// <summary>
        /// 内容推送请求队列
        /// </summary>
        public static readonly string PushHubRequestQueue = "push-hub-request";

        /// <summary>
        ///     Steam Bot 延迟操作队列
        /// </summary>
        public static string SteamBotDelayedActionQueue(string botId) => $"steam-bot-delayed-actions.{botId}";

        /// <summary>
        ///     可靠通知队列
        /// </summary>
        public static string ReliableNotificationQueue(string userId, string hubName)
            => $"reliable-notifications.{hubName}.{userId}";

        #endregion
    }
}