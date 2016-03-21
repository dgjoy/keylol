using System;
using System.Configuration;
using System.Text;
using Keylol.Models.DTO;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供消息队列服务
    /// </summary>
    public static class MessageQueueProvider
    {
        private static IConnection _connection;

        /// <summary>
        /// 获得全局 IConnection 单例
        /// </summary>
        /// <returns>全局 IConnection 单例</returns>
        public static IConnection GetInstance()
        {
            return _connection ?? (_connection = new ConnectionFactory
            {
                Uri = ConfigurationManager.AppSettings["rabbitMqConnection"] ?? "amqp://localhost/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true
            }.CreateConnection());
        }

        /// <summary>
        /// 向队列发送 ImageGarageRequest
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        /// <param name="channel">频道</param>
        public static void SendImageGarageRequest(ImageGarageRequestDto requestDto, IModel channel)
        {
            channel.QueueDeclare("image-garage-requests", true, false, false, null);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish("", "image-garage-requests", properties,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDto)));
        }
    }
}