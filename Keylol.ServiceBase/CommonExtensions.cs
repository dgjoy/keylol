using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Keylol.ServiceBase
{
    /// <summary>
    /// 一些常用的方法扩展
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        ///     向指定队列发送 JSON 序列化后的对象，使用默认 exchange
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