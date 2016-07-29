using System;
using System.Data.Entity;
using System.IdentityModel;
using System.Linq;
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
        private readonly int _creditBase;

        /// <summary>
        /// 创建 <see cref="SteamGiftCardProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public SteamGiftCardProcessor(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _creditBase = 100;
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public override async Task RedeemAsync()
        {
            // 额度检测
            var seasonLiked = await _dbContext.Users.Where(u => u.Id == UserId).Select(u => u.SeasonLikeCount).ToListAsync();
            var boughtTimes =
                _dbContext.CouponGiftOrders
                    .Count(u => u.RedeemTime.Month == DateTime.Now.Month && u.UserId == UserId && u.GiftId == Gift.Id);
            var credit = _creditBase + seasonLiked.First() - boughtTimes * Gift.Price;
            if (credit < Gift.Price)
            {
                throw new BadRequestException();
            }
            
            // 完成兑换
            throw new NotImplementedException();
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