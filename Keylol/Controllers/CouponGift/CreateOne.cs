using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Utilities;
using Newtonsoft.Json;

namespace Keylol.Controllers.CouponGift
{
    public partial class CouponGiftController
    {
        /// <summary>
        /// 创建一个文券商品
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOne([NotNull] CouponGiftCreateOneDto requestDto)
        {
            var gift = new Models.CouponGift
            {
                Type = requestDto.Type,
                Name = requestDto.Name,
                Descriptions = JsonConvert.SerializeObject(requestDto.Descriptions),
                ThumbnailImage = requestDto.ThumbnailImage,
                Price = requestDto.Price,
                Value = requestDto.Value,
                EndTime = requestDto.EndTime
            };
            _dbContext.CouponGifts.Add(gift);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 请求 DTO
        /// </summary>
        public class CouponGiftCreateOneDto
        {
            /// <summary>
            /// 类型
            /// </summary>
            public CouponGiftType Type { get; set; }

            /// <summary>
            /// 名称
            /// </summary>
            [Required]
            public string Name { get; set; }

            /// <summary>
            /// 描述组
            /// </summary>
            [Required]
            public List<string> Descriptions { get; set; }

            /// <summary>
            /// 缩略图
            /// </summary>
            [Required]
            public string ThumbnailImage { get; set; }

            /// <summary>
            /// 文券价格
            /// </summary>
            public int Price { get; set; }

            /// <summary>
            /// 商品价值（用于限定额度）
            /// </summary>
            public int Value { get; set; }

            /// <summary>
            /// 下架时间
            /// </summary>
            public DateTime EndTime { get; set; }
        }
    }
}