using System.Threading.Tasks;
using Keylol.Models;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// 文券商品处理接口
    /// </summary>
    public abstract class GiftProcessor
    {
        /// <summary>
        /// 兑换用户的 ID
        /// </summary>
        protected string UserId;

        /// <summary>
        /// 文券商品
        /// </summary>
        protected CouponGift Gift;

        /// <summary>
        /// 初始化属性
        /// </summary>
        /// <param name="userId">兑换用户的 ID</param>
        /// <param name="gift">文券商品</param>
        public virtual void Initialize(string userId, CouponGift gift)
        {
            UserId = userId;
            Gift = gift;
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public virtual Task RedeemAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public virtual Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            return Task.FromResult(0);
        }
    }
}