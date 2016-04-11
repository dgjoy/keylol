using System.Web.Http;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGiftOrder
{
    /// <summary>
    /// 文券礼品兑换日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift-order")]
    public partial class CouponGiftOrderController : KeylolApiController
    {
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建 <see cref="CouponGiftOrderController"/>
        /// </summary>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public CouponGiftOrderController(CouponProvider coupon)
        {
            _coupon = coupon;
        }
    }
}