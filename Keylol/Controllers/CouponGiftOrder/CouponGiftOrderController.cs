using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.Controllers.CouponGiftOrder
{
    /// <summary>
    /// 文券商品 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift-order")]
    public partial class CouponGiftOrderController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CachedDataProvider _cachedData;
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建<see cref="CouponGiftOrderController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public CouponGiftOrderController(KeylolDbContext dbContext, KeylolUserManager userManager, CachedDataProvider cachedData, CouponProvider coupon)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
            _coupon = coupon;
        }
    }
}
