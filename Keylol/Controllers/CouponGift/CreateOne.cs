using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGift
{
    public partial class CouponGiftController
    {
        /// <summary>
        /// 创建一个文券礼品
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (CouponGiftDto))]
        public async Task<IHttpActionResult> CreateOne(CouponGiftCreateOneRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError(nameof(requestDto), "Invalid request DTO.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gift = DbContext.CouponGifts.Create();
            gift.Name = requestDto.Name;
            gift.Descriptions = JsonConvert.SerializeObject(requestDto.Descriptions);
            gift.PreviewImage = requestDto.PreviewImage;
            gift.AcceptedFields = JsonConvert.SerializeObject(requestDto.AcceptedFields);
            gift.Price = requestDto.Price;
            DbContext.CouponGifts.Add(gift);
            await DbContext.SaveChangesAsync();
            return Created($"coupon-gift/{gift.Id}", new CouponGiftDto(gift));
        }
    }

    /// <summary>
    /// 请求 DTO
    /// </summary>
    public class CouponGiftCreateOneRequestDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Required]
        public List<string> Descriptions { get; set; }

        /// <summary>
        /// 预览图片
        /// </summary>
        [Required]
        public string PreviewImage { get; set; }

        /// <summary>
        /// 接受的用户输入字段
        /// </summary>
        [Required]
        public List<CouponGiftAcceptedFieldDto> AcceptedFields { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
    }
}