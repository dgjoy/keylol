using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Keylol.Models.DAL;

namespace Keylol.Controllers.CouponGiftOrder.CouponGift
{
    /// <summary>
    /// Steam 积分卡
    /// </summary>
    public class SteamGiftCard : IGiftProcessor
    {
        /// <summary>
        /// 人工发放
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dbContext"></param>
        void IGiftProcessor.GiftExchange(string userId, KeylolDbContext dbContext)
        {
            // todo 额度检测


            // todo 人工操作

        }
    }
}