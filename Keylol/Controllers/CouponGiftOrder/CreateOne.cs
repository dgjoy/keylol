using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Controllers.CouponGiftOrder.Processors;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGiftOrder
{
    public partial class CouponGiftOrderController
    {
        /// <summary>
        /// 创建一个文券商品订单
        /// </summary>
        /// <param name="giftId">文券商品 ID</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文券礼品不存在")]
        public async Task<IHttpActionResult> CreateOne(string giftId)
        {
            var gift = await _dbContext.CouponGifts.FindAsync(giftId);
            if (gift == null)
                return NotFound();

            if (DateTime.Now >= gift.EndTime)
                return this.BadRequest(nameof(giftId), Errors.GiftOffTheMarket);

            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user.Coupon < gift.Price)
                return this.BadRequest(nameof(giftId), Errors.NotEnoughCoupon);

            try
            {
                GiftProcessor processor;
                switch (gift.Type)
                {
                    case CouponGiftType.Custom:
                        processor = new CustomProcessor();
                        break;

                    case CouponGiftType.SteamCnCredit:
                        processor = new SteamCnCreditProcessor(_dbContext, _userManager, _coupon);
                        break;

                    case CouponGiftType.SteamGiftCard:
                        processor = new SteamGiftCardProcessor(_dbContext, _userManager, _coupon);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                processor.Initialize(User.Identity.GetUserId(), gift);
                await processor.RedeemAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return this.BadRequest(giftId, e.Message);
            }
        }
    }
}