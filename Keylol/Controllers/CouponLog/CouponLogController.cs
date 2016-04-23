using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.CouponLog
{
    /// <summary>
    ///     文券日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-log")]
    public partial class CouponLogController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="CouponLogController"/>
        /// </summary>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public CouponLogController(CouponProvider coupon, KeylolDbContext dbContext)
        {
            _coupon = coupon;
            _dbContext = dbContext;
        }
    }
}