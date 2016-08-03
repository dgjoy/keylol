using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.CouponGift
{
    /// <summary>
    /// 文券商品 Controller
    /// </summary>
    [Authorize(Roles = KeylolRoles.Operator)]
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