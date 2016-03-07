using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
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

        [Index]
        public int SequenceNumber { get; set; }

        public DateTime LastGameUpdateTime { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public bool LastGameUpdateSucceed { get; set; } = false;

        public bool AutoSubscribeEnabled { get; set; } = true;

        public TimeSpan AutoSubscribeTimeSpan { get; set; } = TimeSpan.FromDays(7);

        #region Auto share options

        public bool AutoShareOnAddingNewFriend { get; set; } = false;

        public bool AutoShareOnUnlockingAchievement { get; set; } = true;

        public bool AutoShareOnAcquiringNewGame { get; set; } = true;

        public bool AutoShareOnJoiningGroup { get; set; } = false;

        public bool AutoShareOnCreatingGroup { get; set; } = false;

        public bool AutoShareOnUpdatingWishlist { get; set; } = false;

        public bool AutoShareOnPublishingReview { get; set; } = true;

        public bool AutoShareOnUploadingScreenshot { get; set; } = false;

        public bool AutoShareOnAddingVideo { get; set; } = false;

        public bool AutoShareOnAddingFavorite { get; set; } = false;

        #endregion

        #region Email notification options

        public bool EmailNotifyOnArticleReplied { get; set; } = false;

        public bool EmailNotifyOnCommentReplied { get; set; } = false;

        public bool EmailNotifyOnEditorRecommended { get; set; } = false;

        public bool EmailNotifyOnMessageReceived { get; set; } = false;

        public bool EmailNotifyOnAdvertisement { get; set; } = false;

        #endregion

        #region Message options

        public bool MessageNotifyOnArticleReplied { get; set; } = true;

        public bool MessageNotifyOnCommentReplied { get; set; } = true;

        public bool MessageNotifyOnEditorRecommended { get; set; } = true;

        public bool MessageNotifyOnArticleLiked { get; set; } = true;

        public bool MessageNotifyOnCommentLiked { get; set; } = true;

        #endregion

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
    }
}