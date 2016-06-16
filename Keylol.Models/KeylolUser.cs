using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Models
{
    public class KeylolUser : IdentityUser
    {
        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(5, MinimumLength = 5)]
        public string IdCode { get; set; }

        [Index]
        public DateTime RegisterTime { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(64)]
        public string RegisterIp { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string GamerTag { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string AvatarImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string HeaderImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(7)]
        public string ThemeColor { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(7)]
        public string LightThemeColor { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string SteamProfileName { get; set; } = string.Empty;

        public DateTime SteamBindingTime { get; set; }

        public PreferredPointName PreferredPointName { get; set; } = PreferredPointName.Chinese;

        [Index]
        public int Coupon { get; set; } = 0;
        
        public string SteamBotId { get; set; }

        public virtual SteamBot SteamBot { get; set; }

        public string InviterId { get; set; }
        public virtual KeylolUser Inviter { get; set; }

        public DateTime LastDailyRewardTime { get; set; } = DateTime.Now;

        public int FreeLike { get; set; } = 5;
        
        #region Steam bot notification options

        public bool SteamNotifyOnArticleReplied { get; set; } = true;

        public bool SteamNotifyOnCommentReplied { get; set; } = true;

        public bool SteamNotifyOnArticleLiked { get; set; } = true;

        public bool SteamNotifyOnCommentLiked { get; set; } = true;

        #endregion
    }

    public enum PreferredPointName
    {
        Chinese,
        English
    }
}