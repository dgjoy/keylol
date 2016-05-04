using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供可靠通知处理的 Hub，可以继承此类实现具有具体功能的 Hub
    /// </summary>
    /// <typeparam name="TClient">Hub Client 类型</typeparam>
    [Authorize]
    public abstract class ReliableHub<TClient> : Hub<TClient> where TClient : class
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<string, IModel> MqChannels =
            new ConcurrentDictionary<string, IModel>();

        /// <summary>
        /// 客户端连接时调用。继承后需要确保 <c>base.OnConnected()</c> 得到调用。
        /// </summary>
        public override async Task OnConnected()
        {
            var mqChannel = Startup.Container.GetInstance<MqClientProvider>().CreateModel();
            MqChannels[Context.ConnectionId] = mqChannel;
            var logger = Startup.Container.GetInstance<LogProvider<ReliableHub<TClient>>>().Logger;
            var userId = Context.User.Identity.GetUserId();
            var queueName = MqClientProvider.ReliableNotificationQueue(userId, GetType().Name);
            mqChannel.QueueDeclare(queueName, true, false, false, null);
            var consumer = new EventingBasicConsumer(mqChannel);
            consumer.Received += (sender, args) =>
            {
                try
                {
                    using (var streamReader = new StreamReader(new MemoryStream(args.Body)))
                    {
                        var serializer = new JsonSerializer();
                        var notification =
                            serializer.Deserialize<ReliableNotificationDto>(new JsonTextReader(streamReader));
                        typeof(TClient).GetMethod(notification.MethodName)
                            .Invoke(Clients.User(userId), notification.Arguments.Select(a =>
                            {
                                var argumentType = Type.GetType(a.Type);
                                if (argumentType == null) return null;
                                var value = a.Value as JToken;
                                return value != null ? value.ToObject(argumentType) : a.Value;
                            }).ToArray());
                        mqChannel.BasicAck(args.DeliveryTag, false);
                    }
                }
                catch (Exception e)
                {
                    mqChannel.BasicNack(args.DeliveryTag, false, false);
                    logger.Fatal("Unhandled MQ consumer exception.", e);
                }
            };
            mqChannel.BasicConsume(queueName, false, consumer);
            await base.OnConnected();
        }

        /// <summary>
        /// 客户端断开时调用。继承后需要确保 <c>base.OnDisconnected()</c> 得到调用。
        /// </summary>
        /// <param name="stopCalled">
        /// true, if stop was called on the client closing the connection gracefully;
        /// false, if the connection has been lost for longer than the
        /// <see cref="P:Microsoft.AspNet.SignalR.Configuration.IConfigurationManager.DisconnectTimeout" />.
        /// Timeouts can be caused by clients reconnecting to another SignalR server in scaleout.
        /// </param>
        public override async Task OnDisconnected(bool stopCalled)
        {
            IModel mqChannel;
            if (MqChannels.TryRemove(Context.ConnectionId, out mqChannel))
                mqChannel.Dispose();
            await base.OnDisconnected(stopCalled);
        }
    }
}