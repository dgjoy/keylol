using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGift
{
    /// <summary>
    /// 文券礼品 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift")]
    public partial class CouponGiftController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="CouponGiftController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public CouponGiftController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}