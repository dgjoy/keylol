using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGiftOrder
{
    /// <summary>
    ///     文券礼品兑换日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift-order")]
    public partial class CouponGiftOrderController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="CouponGiftOrderController" />
        /// </summary>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public CouponGiftOrderController(CouponProvider coupon, KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _coupon = coupon;
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}