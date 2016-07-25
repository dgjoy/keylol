using System;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// Steam 礼品卡
    /// </summary>
    public class SteamGiftCardProcessor : GiftProcessor
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        /// 创建 <see cref="SteamGiftCardProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public SteamGiftCardProcessor(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public override Task RedeemAsync()
        {
            // todo 额度检测

            // todo 人工操作
            throw new NotImplementedException();
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            stateTreeGift.Email = await _userManager.GetEmailAsync(UserId);
        }
    }
}