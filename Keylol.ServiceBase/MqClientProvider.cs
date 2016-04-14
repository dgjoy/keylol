using System;
using System.Collections.Generic;
using System.Configuration;
using log4net;
using RabbitMQ.Client;

namespace Keylol.ServiceBase
{
    /// <summary>
    /// RabbitMQ Client 提供者
    /// </summary>
    public class MqClientProvider : IDisposable
    {
        private readonly ILog _logger;
        private bool _disposed;

        #region 预定义名称

        /// <summary>
        /// 延迟消息交换机
        /// </summary>
        public static readonly string DelayedMessageExchange = "delayed-message-exchange";

        /// <summary>
        ///     Image Garage 请求队列
        /// </summary>
        public static readonly string ImageGarageRequestQueue = "image-garage-requests";

        /// <summary>
        /// Steam Bot 延迟操作队列
        /// </summary>
        public static readonly string SteamBotDelayedActionQueue = "steam-bot-delayed-actions";

        #endregion

        /// <summary>
        /// 创建新 MqClientProvider
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
            }
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _logger.Warn(
                $"RabbitMQ connection shutdown.{(shutdownEventArgs.Cause == null ? string.Empty : $" Reason: {shutdownEventArgs.Cause}")}");
        }

        /// <summary>
        /// RabbitMQ Client IConnection 对象
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        /// 创建新频道
        /// </summary>
        /// <returns>创建的 IModel 对象</returns>
        public IModel CreateModel() => Connection.CreateModel();

        /// <summary>
        /// 资源清理
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 资源清理
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
    }
}