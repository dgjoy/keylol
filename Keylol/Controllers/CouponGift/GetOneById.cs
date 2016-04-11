using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGift
{
    public partial class CouponGiftController
    {
        /// <summary>
        /// 获取指定礼品的详细信息
        /// </summary>
        /// <param name="id">礼品 ID</param>
        [Route("{id}")]
        [HttpGet]
        [ResponseType(typeof (CouponGiftDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定礼品不存在")]
        public async Task<IHttpActionResult> GetOneById(string id)
        {
            var userId = User.Identity.GetUserId();
            var gift = await DbContext.CouponGifts.FindAsync(id);
            if (gift == null)
                return NotFound();
            var order = await DbContext.CouponGiftOrders.FirstOrDefaultAsync(o => o.GiftId == id && o.UserId == userId);
            return Ok(new CouponGiftDto(gift)
            {
                Redeemed = order != null,
                Extra = order == null ? null : JsonConvert.DeserializeObject(order.Extra)
            });
        }
    }
}