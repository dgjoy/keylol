namespace Keylol.Hubs
{
    /// <summary>
    /// 提供邮政通知服务
    /// </summary>
    public class MessageHub : ReliableHub<IMessageHubClient>
    {
    }

    /// <summary>
    /// <see cref="MessageHub"/> Client
    /// </summary>
    public interface IMessageHubClient
    {
        /// <summary>
        /// 通知未读邮政消息变化事件
        /// </summary>
        /// <param name="newCount">新的未读数</param>
        void OnUnreadCountChanged(int newCount);
    }
}