using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGiftOrder
{
    public partial class CouponGiftOrderController
    {
        /// <summary>
        /// 兑换一件文券礼品
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
            {
                {
                    ModelState.AddModelError(nameof(giftId), "礼品已经下架，无法兑换");
                    return BadRequest(ModelState);
                }
            }

            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.Where(u => u.Id == userId).SingleAsync();
            if (user.Coupon - gift.Price < 0)
            {
                ModelState.AddModelError("userId", "文券不足，无法兑换");
                return BadRequest(ModelState);
            }

            if (await _dbContext.CouponGiftOrders.Where(o => o.UserId == userId && o.GiftId == giftId).AnyAsync())
            {
                ModelState.AddModelError("userId", "已经兑换过这个礼品，无法重复兑换");
                return BadRequest(ModelState);
            }

            var order = _dbContext.CouponGiftOrders.Create();
            order.UserId = userId;
            order.GiftId = gift.Id;
            var sanitizedExtra = new JObject();
            var acceptedFields = JsonConvert.DeserializeObject<List<CouponGiftAcceptedFieldDto>>(gift.AcceptedFields);
            foreach (var field in acceptedFields)
            {
                if (extra[field.Id] == null)
                {
                    ModelState.AddModelError(nameof(extra), "缺失必要的额外输入属性");
                    return BadRequest(ModelState);
                }
                sanitizedExtra[field.Id] = extra[field.Id];
            }
            order.Extra = JsonConvert.SerializeObject(sanitizedExtra);
            _dbContext.CouponGiftOrders.Add(order);
            await _dbContext.SaveChangesAsync();
            await _coupon.Update(userId, CouponEvent.兑换商品, -gift.Price, new {CouponGiftId = giftId});
            return Created($"coupon-gift-order/{order.Id}", string.Empty);
        }
    }
}