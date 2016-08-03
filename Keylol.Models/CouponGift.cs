using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class CouponGift
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        public CouponGiftType Type { get; set; } = CouponGiftType.Custom;

        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     JSON 数组
        /// </summary>
        [Required]
        public string Descriptions { get; set; } = "[]";

        [Required]
        public string ThumbnailImage { get; set; }

        [Obsolete]
        public string PreviewImage { get; set; }

        /// <summary>
        ///     JSON 数组，元素示例：
        ///     { Id: "UID", Title: "蒸汽动力用户编号", Description: "论坛用户码（UID），在资料中可以找到。", InputType: "number" }
        /// </summary>
        [Required]
        public string AcceptedFields { get; set; } = "[]";

        public int Price { get; set; } = 10;

        public int Value { get; set; } = 100;

        [Index]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime EndTime { get; set; }
    }

    public enum CouponGiftType
    {
        Custom,
        SteamCnCredit,
        SteamGiftCard
    }
}