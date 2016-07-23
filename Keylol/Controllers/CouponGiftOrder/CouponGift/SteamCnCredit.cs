using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Keylol.Models.DAL;

namespace Keylol.Controllers.CouponGiftOrder.CouponGift
{
    /// <summary>
    /// SteamCn 论坛积分
    /// </summary>
    public class SteamCnCredit : IGiftProcessor
    {
        /// <summary>
        /// 自动兑换
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dbContext"></param>
        void IGiftProcessor.GiftExchange(string userId, KeylolDbContext dbContext)
        {
            // todo 额度检测

            // todo 即时充值
        }
    }
}