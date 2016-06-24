using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGiftOrder
{
    public partial class CouponGiftOrderController
    {
        /// <summary>
        ///     兑换一件文券礼品
        /// </summary>
        /// <param name="giftId">礼品 ID</param>
        /// <param name="extra">用户输入的额外属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文券礼品不存在")]
        public async Task<IHttpActionResult> CreateOne(string giftId, JObject extra)
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

            if (await _dbContext.CouponGiftOrders.Where(o => o.UserId == userId && o.GiftId == giftId).AnyAsync())
                return this.BadRequest(nameof(giftId), Errors.GiftOwned);

            var order = _dbContext.CouponGiftOrders.Create();
            order.UserId = userId;
            order.GiftId = gift.Id;
            var sanitizedExtra = new JObject();
            var acceptedFields = JsonConvert.DeserializeObject<List<CouponGiftAcceptedFieldDto>>(gift.AcceptedFields);
            foreach (var field in acceptedFields)
            {
                if (extra[field.Id] == null)
                    return this.BadRequest(nameof(extra), nameof(field.Id), Errors.Required);

                sanitizedExtra[field.Id] = extra[field.Id];
            }
            order.Extra = JsonConvert.SerializeObject(sanitizedExtra);
            _dbContext.CouponGiftOrders.Add(order);
            await _dbContext.SaveChangesAsync();
            await _coupon.Update(user, CouponEvent.兑换商品, -gift.Price, new {CouponGiftId = giftId});
            return Created($"coupon-gift-order/{order.Id}", string.Empty);
        }
    }
}