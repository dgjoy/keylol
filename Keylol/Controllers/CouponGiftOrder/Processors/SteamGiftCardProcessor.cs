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
        private readonly int _creditBase;

        /// <summary>
        /// 创建 <see cref="SteamGiftCardProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="coupon"></param>
        public SteamGiftCardProcessor(KeylolDbContext dbContext, KeylolUserManager userManager, CouponProvider coupon)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _coupon = coupon;
            _creditBase = 0;
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public override async Task RedeemAsync()
        {
            // 额度检测
            var user = await _userManager.FindByIdAsync(UserId);
            var seasonLikeCount = user.SeasonLikeCount;
            var boughtTimes =
                _dbContext.CouponGiftOrders
                    .Count(u => u.RedeemTime.Month == DateTime.Now.Month && u.UserId == UserId && u.GiftId == Gift.Id);
            var credit = _creditBase + seasonLikeCount - boughtTimes * Gift.Price;
            if (credit < Gift.Price)
            {
                throw new BadRequestException(nameof(credit));
            }
            
            // 电邮地址检测
            var email = user.Email;
            if (email == null)
            {
                throw new BadRequestException(nameof(email));
            }

            // 完成兑换
            var order = _dbContext.CouponGiftOrders.Create();
            order.UserId = UserId;
            order.GiftId = Gift.Id;
            _dbContext.CouponGiftOrders.Add(order);
            try
            {
                await _coupon.UpdateAsync(user, CouponEvent.兑换商品, -Gift.Price, new { CouponGiftId = Gift.Id });
                user.Coupon = user.Coupon - Gift.Price;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(_coupon),e.InnerException);
            }
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            stateTreeGift.Email = await _userManager.GetEmailAsync(UserId);
            var seasonLiked = await _dbContext.Users.Where(u => u.Id == UserId).Select(u=>u.SeasonLikeCount).ToListAsync();
            var boughtTimes =
                _dbContext.CouponGiftOrders
                    .Count(u => u.RedeemTime.Month == DateTime.Now.Month && u.UserId == UserId && u.GiftId == Gift.Id);
            stateTreeGift.Credit = _creditBase + seasonLiked.First() - boughtTimes * Gift.Price;
        }
    }
}