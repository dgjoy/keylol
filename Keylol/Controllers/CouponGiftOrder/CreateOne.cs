using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Keylol.Controllers.CouponGiftOrder.CouponGift;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGiftOrder
{
    public partial class CouponGiftOrderController
    {
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

            // add gift order into database
            var type = gift.Type;
            try
            {
                IGiftProcessor giftProcessor;
                switch (type)
                {
                    case CouponGiftType.Custom:
                        giftProcessor = new Custom();
                        giftProcessor.GiftExchange(userId,_dbContext);
                        break;

                    case CouponGiftType.SteamCnCredit:
                        giftProcessor = new SteamCnCredit();
                        giftProcessor.GiftExchange(userId, _dbContext);
                        break;

                    case CouponGiftType.SteamGiftCard:
                        giftProcessor = new SteamGiftCard();
                        giftProcessor.GiftExchange(userId, _dbContext);
                        break;

                    default:
                        throw new BadRequestException("错误的 gift 类型");
                }
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message, Errors.Invalid);
            }
            return Ok();
        }

    }

}