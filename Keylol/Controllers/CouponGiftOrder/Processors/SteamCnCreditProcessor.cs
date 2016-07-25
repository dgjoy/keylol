using System;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// SteamCN 论坛积分
    /// </summary>
    public class SteamCnCreditProcessor : GiftProcessor
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        
        /// <summary>
        /// 创建 <see cref="SteamCnCreditProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public SteamCnCreditProcessor(KeylolDbContext dbContext, KeylolUserManager userManager)
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

            // todo 即时充值
            throw new NotImplementedException();
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            stateTreeGift.SteamCnUid = await _userManager.GetSteamCnUidAsync(UserId);
            var user = await _userManager.FindByIdAsync(UserId);
            stateTreeGift.SteamCnUserName = user.SteamCnUserName;
        }
    }
}