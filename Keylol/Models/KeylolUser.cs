using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    // You can add profile data for the user by adding more properties to your KeylolUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class KeylolUser : IdentityUser
    {
        public DateTime RegisterTime { get; set; } = DateTime.Now;
        public string RegisterIp { get; set; }
        public DateTime LastVisitTime { get; set; } = DateTime.Now;
        public string LastVisitIp { get; set; }
        public virtual ICollection<Point> SubscribedPoints { get; set; }

        [Required]
        public virtual ProfilePoint ProfilePoint { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<UserMessage> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Warning> SentWarnings { get; set; }
        public virtual ICollection<Warning> ReceivedWarnings { get; set; }
        public virtual ICollection<NormalPoint> ModeratedPoints { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<KeylolUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        #region Preferences

        [MaxLength(100)]
        public string GamerTag { get; set; }

        // Auto share options
        public bool AutoShareOnAcquiringNewGame { get; set; } = true;
        public bool AutoShareOnPublishingReview { get; set; } = true;
        public bool AutoShareOnUnlockingAchievement { get; set; } = true;
        public bool AutoShareOnUploadingScreenshot { get; set; } = true;
        public bool AutoShareOnAddingFavorite { get; set; } = true;

        // Email notification options
        public bool EmailNotifyOnArticleReplied { get; set; } = true;
        public bool EmailNotifyOnCommentReplied { get; set; } = true;
        public bool EmailNotifyOnEditorRecommended { get; set; } = true;
        public bool EmailNotifyOnUserMessageReceived { get; set; } = true;

        public LanguageConversionMode PreferedLanguageConversionMode { get; set; } = LanguageConversionMode.SimplifiedChineseWithContentUnmodified;

        // Accessibility demand
        public bool ColorVisionDeficiency { get; set; } = false;
        public bool VisionImpairment { get; set; } = false;
        public bool HearingImpairment { get; set; } = false;
        public bool PhotosensitiveEpilepsy { get; set; } = false;

        #endregion
    }
}