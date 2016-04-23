using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGiftOrder
{
    /// <summary>
    /// 文券礼品兑换日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift-order")]
    public partial class CouponGiftOrderController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="CouponGiftOrderController"/>
        /// </summary>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public CouponGiftOrderController(CouponProvider coupon, KeylolDbContext dbContext)
        {
            _coupon = coupon;
            _dbContext = dbContext;
        }
    }
}