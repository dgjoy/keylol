using Keylol.Models;

namespace Keylol.Hubs
{
    /// <summary>
    /// 提供文券通知服务
    /// </summary>
    public class CouponHub : ReliableHub<ICouponHubClient>
    {
    }

    /// <summary>
    /// <see cref="CouponHub"/> Client
    /// </summary>
    public interface ICouponHubClient
    {
        /// <summary>
        /// 通知文券变化事件
        /// </summary>
        /// <param name="event">事件</param>
        /// <param name="change">变化量</param>
        /// <param name="balance">余额</param>
        void OnCouponChanged(CouponEvent @event, int change, int balance);
    }
}