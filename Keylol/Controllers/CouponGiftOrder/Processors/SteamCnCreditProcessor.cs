using System;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// SteamCN 论坛积分
    /// </summary>
    public class SteamCnCreditProcessor : GiftProcessor
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CouponProvider _coupon;
        private readonly int _creditBase;

        /// <summary>
        /// 创建 <see cref="SteamCnCreditProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public SteamCnCreditProcessor(KeylolDbContext dbContext, KeylolUserManager userManager, CouponProvider coupon)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _coupon = coupon;
            _creditBase = 120;
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
            stateTreeGift.Credit = _creditBase;
        }
    }
}