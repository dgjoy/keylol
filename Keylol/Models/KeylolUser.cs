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
        public KeylolUser()
        {
            RegisterTime = DateTime.Now;
            LastVisitTime = DateTime.Now;
            AutoShareOnAcquiringNewGame = true;
            AutoShareOnPublishingReview = true;
            AutoShareOnUnlockingAchievement = true;
            AutoShareOnUploadingScreenshot = true;
            AutoShareOnAddingFavorite = true;
            EmailNotifyOnArticleReplied = true;
            EmailNotifyOnCommentReplied = true;
            EmailNotifyOnEditorRecommended = true;
            EmailNotifyOnUserMessageReceived = true;
            PreferedLanguageConversionMode = LanguageConversionMode.SimplifiedChineseWithContentUnmodified;
            ColorVisionDeficiency = false;
            VisionImpairment = false;
            HearingImpairment = false;
            PhotosensitiveEpilepsy = false;
        }

        public DateTime RegisterTime { get; set; }
        public string RegisterIp { get; set; }
        public DateTime LastVisitTime { get; set; }
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

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<KeylolUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        #region Preferences

        [MaxLength(100)]
        public string GamerTag { get; set; }

        // Auto share options
        public bool AutoShareOnAcquiringNewGame { get; set; }
        public bool AutoShareOnPublishingReview { get; set; }
        public bool AutoShareOnUnlockingAchievement { get; set; }
        public bool AutoShareOnUploadingScreenshot { get; set; }
        public bool AutoShareOnAddingFavorite { get; set; }

        // Email notification options
        public bool EmailNotifyOnArticleReplied { get; set; }
        public bool EmailNotifyOnCommentReplied { get; set; }
        public bool EmailNotifyOnEditorRecommended { get; set; }
        public bool EmailNotifyOnUserMessageReceived { get; set; }

        public LanguageConversionMode PreferedLanguageConversionMode { get; set; }

        // Accessibility demand
        public bool ColorVisionDeficiency { get; set; }
        public bool VisionImpairment { get; set; }
        public bool HearingImpairment { get; set; }
        public bool PhotosensitiveEpilepsy { get; set; }

        #endregion
    }
}