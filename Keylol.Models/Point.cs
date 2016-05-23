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

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string Logo { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(7)]
        public string ThemeColor { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(7)]
        public string LightThemeColor { get; set; } = string.Empty;

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

        public string OriginLink { get; set; }

        public double? OriginPrice { get; set; }

        public string WindowsStoreLink { get; set; }

        public double? WindowsStorePrice { get; set; }

        public string AppStoreLink { get; set; }

        public double? AppStorePrice { get; set; }

        public string GooglePlayLink { get; set; }

        public double? GooglePlayPrice { get; set; }

        public string GogLink { get; set; }

        public double? GogPrice { get; set; }

        public string BattleNetLink { get; set; }

        public double? BattleNetPrice { get; set; }

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

        [Index]
        public bool InAppPurchases { get; set; }

        #endregion

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string DisplayAliases { get; set; } = string.Empty;

        public DateTime? PublishDate { get; set; }

        [Index]
        public DateTime? ReleaseDate { get; set; }

        public DateTime? PreOrderDate { get; set; }

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

        #region Steam Spy 数据

        public int? OwnerCount { get; set; }

        public int? OwnerCountVariance { get; set; }

        public int? TotalPlayerCount { get; set; }

        public int? TotalPlayerCountVariance { get; set; }

        public int? TwoWeekPlayerCount { get; set; }

        public int? TwoWeekPlayerCountVariance { get; set; }

        public int? AveragePlayedTime { get; set; }

        public int? TwoWeekAveragePlayedTime { get; set; }

        public int? MedianPlayedTime { get; set; }

        public int? TwoWeekMedianPlayedTime { get; set; }

        public int? Ccu { get; set; }

        #endregion

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