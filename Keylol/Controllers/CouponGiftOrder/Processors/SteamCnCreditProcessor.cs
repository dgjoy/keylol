using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Utilities;

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
        private const int CreditBase = 120;

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
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public override async Task RedeemAsync()
        {
            if (await GetCreditAsync() < Gift.Price)
                throw new Exception(Errors.NotEnoughCredit);

            if (await _userManager.GetSteamCnUidAsync(User.Id) == null)
                throw new Exception(Errors.SteamCnAccountNotBound);

            throw new NotImplementedException();

/*
            _dbContext.CouponGiftOrders.Add(new Models.CouponGiftOrder
            {
                UserId = User.Id,
                GiftId = Gift.Id,
                RedeemPrice = Gift.Price,
                Finished = true
            });
            await _dbContext.SaveChangesAsync();
            await _coupon.UpdateAsync(User, CouponEvent.兑换商品, -Gift.Price, new { CouponGiftId = Gift.Id });
*/
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            stateTreeGift.SteamCnUserName = User.SteamCnUserName;
            stateTreeGift.SteamCnUid = await _userManager.GetSteamCnUidAsync(User.Id);
            stateTreeGift.Credit = await GetCreditAsync();
        }

        private async Task<int> GetCreditAsync()
        {
            var boughtCredits = await _dbContext.CouponGiftOrders.Where(giftOrder =>
                giftOrder.RedeemTime.Month == DateTime.Now.Month && giftOrder.UserId == User.Id &&
                giftOrder.Gift.Type == Gift.Type).Select(o => o.Gift.Value).DefaultIfEmpty(0).SumAsync();

            return CreditBase - boughtCredits;
        }
    }
}