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

        [Required]
        [Index(IsUnique = true)]
        [StringLength(5, MinimumLength = 5)]
        public string IdCode { get; set; }

        [Index]
        public PointType Type { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Index]
        public DateTime LastActivityTime { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(150)]
        public string EnglishName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(150)]
        public string ChineseName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string EnglishAliases { get; set; } = string.Empty; // Comma separated list

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string ChineseAliases { get; set; } = string.Empty; // Comma separated list

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string AvatarImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string HeaderImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(10000)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<SteamStoreName> SteamStoreNames { get; set; }

        #region 游戏、硬件据点属性

        #region 商店信息

        [Index]
        public int? SteamAppId { get; set; }

        public double? SteamPrice { get; set; }

        public double? SteamDiscountedPrice { get; set; }

        public int? SonkwoProductId { get; set; }

        public double? SonkwoPrice { get; set; }

        public double? SonkwoDiscountedPrice { get; set; }

        public string UplayLink { get; set; }

        public double? UplayPrice { get; set; }

        public string XboxLink { get; set; }

        public double? XboxPrice { get; set; }

        public string PlayStationLink { get; set; }

        public double? PlayStationPrice { get; set; }

        #endregion

        #region 特性属性

        [Index]
        public bool MultiPlayer { get; set; }

        [Index]
        public bool SinglePlayer { get; set; }

        [Index]
        public bool Coop { get; set; }

        [Index]
        public bool CaptionsAvailable { get; set; }

        [Index]
        public bool CommentaryAvailable { get; set; }

        [Index]
        public bool IncludeLevelEditor { get; set; }

        [Index]
        public bool Achievements { get; set; }

        [Index]
        public bool Cloud { get; set; }

        [Index]
        public bool LocalCoop { get; set; }

        [Index]
        public bool SteamTradingCards { get; set; }

        [Index]
        public bool SteamWorkshop { get; set; }

        #endregion

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string DisplayAliases { get; set; } = string.Empty;

        [Index]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string TitleCoverImage { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string ThumbnailImage { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string MediaHeaderImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string Media { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string ChineseAvailability { get; set; } = string.Empty;

        #endregion

        #region 平台据点

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string EmblemImage { get; set; } = string.Empty;

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