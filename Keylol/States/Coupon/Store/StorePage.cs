using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Coupon.Store
{
    /// <summary>
    /// 文券商品页面
    /// </summary>
    public class StorePage
    {
        /// <summary>
        /// 文券商品页面返回结果
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="cachedData"></param>
        /// <returns></returns>
        public static async Task<StorePage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="StorePage"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<StorePage> CreateAsync([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return new StorePage
            {
                Results = await CouponGiftList.CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData)
            };
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        public CouponGiftList Results { get; set; }
    }

}