using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;

namespace Keylol.Controllers.CouponGift
{
    public partial class CouponGiftController
    {
        /// <summary>
        /// 获取全部文券礼品列表
        /// </summary>
        [Route]
        [HttpGet]
        [ResponseType(typeof (List<CouponGiftDto>))]
        public async Task<IHttpActionResult> GetListOfAll()
        {
            return Ok((await DbContext.CouponGifts.Where(g => DateTime.Now < g.EndTime)
                .OrderByDescending(g => g.CreateTime)
                .Select(g => new
            {
                gift = g,
                redeemCount = DbContext.CouponGiftOrders.Count(o => o.GiftId == g.Id)
            }).ToListAsync())
                .Select(e => new CouponGiftDto
                {
                    Id = e.gift.Id,
                    Price = e.gift.Price,
                    Name = e.gift.Name,
                    ThumbnailImage = e.gift.ThumbnailImage,
                    RedeemCount = e.redeemCount
                }));
        }
    }
}