using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
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

        /// <summary>
        /// 创建<see cref="CouponGiftOrderController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public CouponGiftOrderController(KeylolDbContext dbContext, KeylolUserManager userManager,
            CachedDataProvider cachedData)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
        }
    }

    /// <summary>
    /// 文券商品处理接口
    /// </summary>
    public interface IGiftProcessor
    {
        /// <summary>
        /// 商品兑换
        /// </summary>
        /// <param name="userId">用户 Id</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        void GiftExchange(string userId, KeylolDbContext dbContext);
        
    }
}
