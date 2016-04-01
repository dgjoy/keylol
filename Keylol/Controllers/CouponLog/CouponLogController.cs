using System.Web.Http;
using Keylol.Provider;

namespace Keylol.Controllers.CouponLog
{
    /// <summary>
    ///     文券日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-log")]
    public partial class CouponLogController : KeylolApiController
    {
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建 <see cref="CouponLogController"/>
        /// </summary>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public CouponLogController(CouponProvider coupon)
        {
            _coupon = coupon;
        }
    }
}