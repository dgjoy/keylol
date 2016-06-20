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

        [Index]
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

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string UplayLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string UplayPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string XboxLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string XboxPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string PlayStationLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string PlayStationPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string OriginLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string OriginPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string WindowsStoreLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string WindowsStorePrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string AppStoreLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string AppStorePrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string GooglePlayLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string GooglePlayPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string GogLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string GogPrice { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string BattleNetLink { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string BattleNetPrice { get; set; } = string.Empty;

        #endregion

        #region 特性属性
        
        public bool MultiPlayer { get; set; }
        
        public bool SinglePlayer { get; set; }
        
        public bool Coop { get; set; }
        
        public bool CaptionsAvailable { get; set; }
        
        public bool CommentaryAvailable { get; set; }
        
        public bool IncludeLevelEditor { get; set; }
        
        public bool Achievements { get; set; }
        
        public bool Cloud { get; set; }
        
        public bool LocalCoop { get; set; }
        
        public bool SteamTradingCards { get; set; }
        
        public bool SteamWorkshop { get; set; }
        
        public bool InAppPurchases { get; set; }

        #endregion

        public DateTime? PublishDate { get; set; }
        
        public DateTime? ReleaseDate { get; set; }

        public DateTime? PreOrderDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string TitleCoverImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string ThumbnailImage { get; set; } = string.Empty;

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