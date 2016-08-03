using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// Steam 礼品卡
    /// </summary>
    public class SteamGiftCardProcessor : GiftProcessor
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CouponProvider _coupon;
        private const int CreditBase = 0;

        /// <summary>
        /// 创建 <see cref="SteamGiftCardProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public SteamGiftCardProcessor(KeylolDbContext dbContext, KeylolUserManager userManager, CouponProvider coupon)
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
            var user = await _userManager.FindByIdAsync(UserId);

            if (await GetCreditAsync(user) < Gift.Price) throw new Exception(Errors.NotEnoughCredit);
            if (user.Email == null) throw new Exception(Errors.EmailNonExistent);
            _dbContext.CouponGiftOrders.Add(new Models.CouponGiftOrder
            {
                UserId = UserId,
                GiftId = Gift.Id,
                RedeemPrice = Gift.Price
            });
            await _dbContext.SaveChangesAsync();
            await _coupon.UpdateAsync(user, CouponEvent.兑换商品, -Gift.Price, new { CouponGiftId = Gift.Id });
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            stateTreeGift.Email = user.Email;
            stateTreeGift.Credit = await GetCreditAsync(user);
            stateTreeGift.Coupon = user.Coupon;
        }

        private async Task<int> GetCreditAsync(KeylolUser user)
        {
            var boughtCredits = await _dbContext.CouponGiftOrders.Where(giftOrder =>
                giftOrder.RedeemTime.Month == DateTime.Now.Month && giftOrder.UserId == UserId &&
                giftOrder.Gift.Type == Gift.Type).Select(o=>o.RedeemPrice).DefaultIfEmpty(0).SumAsync();

            return CreditBase + user.SeasonLikeCount - boughtCredits;
        }
    }
}