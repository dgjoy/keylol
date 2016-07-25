using System;
using System.Threading.Tasks;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// 自定义商品
    /// </summary>
    public class CustomProcessor : GiftProcessor
    {
        /// <summary>
        /// 商品兑换
        /// </summary>
        public override Task RedeemAsync()
        {
            throw new NotImplementedException();
        }
    }
}