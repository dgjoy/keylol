using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class CouponGiftOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual KeylolUser User { get; set; }

        [Required]
        public string GiftId { get; set; }

        public virtual CouponGift Gift { get; set; }

        [Index]
        public DateTime RedeemTime { get; set; } = DateTime.Now;

        /// <summary>
        ///     用户额外录入的信息，JSON 格式，按照 <see cref="CouponGift" /> AcceptedFields 录入
        /// </summary>
        [Required]
        public string Extra { get; set; } = "{}";

        public bool Finished { get; set; } = false;

        [Required]
        public int CurrentPrice { get; set; }
    }
}