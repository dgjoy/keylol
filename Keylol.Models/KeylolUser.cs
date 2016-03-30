using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Models
{
    public enum LanguageConversionMode
    {
        ForceSimplifiedChinese,
        ForceTraditionalChinese,
        SimplifiedChineseWithContentUnmodified,
        TraditionalChineseWithContentUnmodified
    }

    public class KeylolUser : IdentityUser
    {
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
        [MaxLength(64)]
        public string AvatarImage { get; set; } = string.Empty;

        public override bool LockoutEnabled { get; set; } = true;

        [Index]
        [MaxLength(64)]
        public string SteamId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string SteamProfileName { get; set; } = string.Empty;

        public DateTime SteamBindingTime { get; set; }

        public DateTime LastGameUpdateTime { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public bool LastGameUpdateSucceed { get; set; } = false;

        public bool AutoSubscribeEnabled { get; set; } = true;

        public int AutoSubscribeDaySpan { get; set; } = 7;

        public virtual ProfilePoint ProfilePoint { get; set; }

        public string ProfilePointId => Id;

        public virtual ICollection<Point> SubscribedPoints { get; set; }

        public virtual ICollection<NormalPoint> ManagedPoints { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Like> Likes { get; set; }

        public virtual ICollection<LoginLog> LoginLogs { get; set; }

        public virtual ICollection<EditLog> EditLogs { get; set; }

        public string SteamBotId { get; set; }

        public virtual SteamBot SteamBot { get; set; }

        public virtual InvitationCode InvitationCode { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int SequenceNumber { get; set; }

        //        public LanguageConversionMode PreferedLanguageConversionMode { get; set; } =
        //            LanguageConversionMode.SimplifiedChineseWithContentUnmodified;

        // Accessibility demand
        //        public bool ColorVisionDeficiency { get; set; } = false;
        //        public bool VisionImpairment { get; set; } = false;
        //        public bool HearingImpairment { get; set; } = false;
        //        public bool PhotosensitiveEpilepsy { get; set; } = false;

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<KeylolUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        #region Steam bot notification options

        public bool SteamNotifyOnArticleReplied { get; set; } = true;

        public bool SteamNotifyOnCommentReplied { get; set; } = true;

        public bool SteamNotifyOnArticleLiked { get; set; } = true;

        public bool SteamNotifyOnCommentLiked { get; set; } = true;

        #endregion
    }
}