using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Point
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Index]
        public PointType Type { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Index]
        public DateTime LastActivityTime { get; set; } = DateTime.Now;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string BackgroundImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string AvatarImage { get; set; } = string.Empty;

        [Required]
        [Index(IsUnique = true)]
        [StringLength(5, MinimumLength = 5)]
        public string IdCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string EnglishName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(150)]
        public string ChineseName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(150)]
        public string NameInSteamStore { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string EnglishAliases { get; set; } = string.Empty; // Comma separated list

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string ChineseAliases { get; set; } = string.Empty; // Comma separated list

        [Required(AllowEmptyStrings = true)]
        [MaxLength(10000)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<SteamStoreName> SteamStoreNames { get; set; }

        #region 游戏、硬件据点属性

        [Index]
        public int SteamAppId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string DisplayAliases { get; set; } = string.Empty;

        [Index]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string CoverImage { get; set; } = string.Empty;
        
        #endregion
    }

    public enum PointType
    {
        Game,
        Category,
        Vendor,
        Platform,
        Hardware
    }
}