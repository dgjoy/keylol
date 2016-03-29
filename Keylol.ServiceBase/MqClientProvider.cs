using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Keylol.ServiceBase
{
    /// <summary>
    /// RabbitMQ Client 提供者
    /// </summary>
    public class MqClientProvider : IDisposable
    {
        private readonly ILog _logger;

        #region 预定义队列名称

        /// <summary>
        ///     ImageGarage 请求队列
        /// </summary>
        public static readonly string ImageGarageRequestQueue = "image-garage-requests";

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
            ((IRecoverable) Connection).Recovery += (sender, eventArgs) =>
            {
                using (NDC.Push("RabbitMQ"))
                    _logger.Info("Connection recovered.");
            };
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            using (NDC.Push("RabbitMQ"))
                _logger.Warn(
                    $"Connection shutdown.{(shutdownEventArgs.Cause == null ? string.Empty : $" Reason: {shutdownEventArgs.Cause}")}");
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
            if (disposing)
            {
                Connection.ConnectionShutdown -= OnConnectionShutdown;
                Connection.Dispose();
                using (NDC.Push("RabbitMQ"))
                    _logger.Info("Connection closed.");
            }
        }
    }
}