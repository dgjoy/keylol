using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.Like
{
    /// <summary>
    /// 认可 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("like")]
    public partial class LikeController : KeylolApiController
    {
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建 LikeController
        /// </summary>
        /// <param name="coupon">CouponProvider</param>
        public LikeController(CouponProvider coupon)
        {
            _coupon = coupon;
        }
    }
}