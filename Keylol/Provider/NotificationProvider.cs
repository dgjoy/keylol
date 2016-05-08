using System;
using System.Linq;
using System.Linq.Expressions;
using Keylol.Hubs;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RabbitMQ.Client;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供实时通知服务
    /// </summary>
    public class NotificationProvider : IDisposable
    {
        private bool _disposed;
        private readonly IModel _mqChannel;

        /// <summary>
        /// 创建 <see cref="NotificationProvider"/>
        /// </summary>
        /// <param name="mqClientProvider"></param>
        public NotificationProvider(MqClientProvider mqClientProvider)
        {
            _mqChannel = mqClientProvider.CreateModel();
        }

        /// <summary>
        /// 获取指定 SignalR Hub Connection Context
        /// </summary>
        /// <typeparam name="THub">Hub 类型</typeparam>
        /// <typeparam name="TClient">Hub Client 类型</typeparam>
        /// <returns><see cref="IHubConnectionContext{TClient}"/></returns>
        public static IHubConnectionContext<TClient> Hub<THub, TClient>()
            where THub : Hub<TClient>
            where TClient : class
        {
            return GlobalHost.ConnectionManager.GetHubContext<THub, TClient>().Clients;
        }

        /// <summary>
        /// 对指定用户发出通知，并保证通知送达（如果用户当前不在线则存储通知，下次上线时发送）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="clientActionExpression">通知发送表达式</param>
        /// <typeparam name="THub">Hub 类型</typeparam>
        /// <typeparam name="TClient">Hub Client 类型</typeparam>
        public void ReliableNotify<THub, TClient>(string userId,
            Expression<Action<TClient>> clientActionExpression)
            where THub : ReliableHub<TClient>
            where TClient : class
        {
            var methodCallExpression = (MethodCallExpression) clientActionExpression.Body;
            var hubType = typeof(THub);
            var dto = new ReliableNotificationDto
            {
                MethodName = methodCallExpression.Method.Name,
                Arguments = methodCallExpression.Arguments
                    .Select(argument => new ReliableNotificationArgumentDto
                    {
                        Type = $"{argument.Type.FullName}, {argument.Type.Assembly.GetName().Name}",
                        Value = Expression.Lambda(argument).Compile().DynamicInvoke()
                    })
                    .ToList()
            };
            var queueName = MqClientProvider.ReliableNotificationQueue(userId, hubType.Name);
            _mqChannel.QueueDeclare(queueName, true, false, false, null);
            _mqChannel.SendMessage(string.Empty, queueName, dto);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     资源清理
        /// </summary>
        /// <param name="disposing">是否清理托管对象</param>
        protected void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _mqChannel.Dispose();
            }
            _disposed = true;
        }
    }
}