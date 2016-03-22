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

        #region 预定义队列名称

        /// <summary>
        /// ImageGarage 请求队列
        /// </summary>
        public static readonly string ImageGarageRequestQueue = "image-garage-requests";

        #endregion

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
        /// 从全局 IConnection 创建新 Model 并返回
        /// </summary>
        /// <returns>创建的 IModel 对象</returns>
        public static IModel CreateModel()
        {
            return GetInstance().CreateModel();
        }

        /// <summary>
        /// 向指定队列发送 JSON 序列化后的对象，使用默认 exchange
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="requestDto">请求 DTO</param>
        public static IModel SendRequest<TDto>(this IModel channel, string queueName, TDto requestDto)
        {
            channel.QueueDeclare(queueName, true, false, false, null);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish("", queueName, properties,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDto)));
            return channel;
        }
    }
}