using System.Web.Http;
using Keylol.Identity;
using Keylol.Provider;

namespace Keylol.Controllers.CouponLog
{
    /// <summary>
    ///     文券日志 Controller
    /// </summary>
    [Authorize(Roles = KeylolRoles.Operator)]
    [RoutePrefix("coupon-log")]
    public partial class CouponLogController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="CouponLogController" />
        /// </summary>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public CouponLogController(CouponProvider coupon, KeylolUserManager userManager)
        {
            _coupon = coupon;
            _userManager = userManager;
        }
    }
}