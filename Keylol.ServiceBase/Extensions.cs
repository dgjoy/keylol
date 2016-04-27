using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Keylol.ServiceBase
{
    /// <summary>
    ///     一些常用的方法扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     向指定队列发送 JSON 序列化后的对象
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routingKey">路由 Key</param>
        /// <param name="requestDto">请求 DTO</param>
        /// <param name="delay">延迟投递的时间，单位毫秒，默认为 0，表示不使用延迟投递</param>
        public static IModel SendMessage<TDto>(this IModel channel, string exchange, string routingKey, TDto requestDto,
            long delay = 0)
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            if (delay > 0)
            {
                properties.Headers = new Dictionary<string, object> {{"x-delay", delay}};
            }
            channel.BasicPublish(exchange, routingKey, properties,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDto)));
            return channel;
        }
    }
}