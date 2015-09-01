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

    // You can add profile data for the user by adding more properties to your KeylolUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public sealed class KeylolUser : IdentityUser
    {
        public KeylolUser()
        {
            LockoutEnabled = true;
        }

        public KeylolUser(string userName) : this()
        {
            UserName = userName;
        }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(5, MinimumLength = 5)]
        public string IdCode { get; set; }

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

        // Auto share options
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

        // Email notification options
        public bool EmailNotifyOnArticleReplied { get; set; } = false;
        public bool EmailNotifyOnCommentReplied { get; set; } = false;
        public bool EmailNotifyOnEditorRecommended { get; set; } = false;
        public bool EmailNotifyOnMessageReceived { get; set; } = false;
        public bool EmailNotifyOnAdvertisement { get; set; } = false;

        // Message options
        public bool MessageNotifyOnArticleReplied { get; set; } = true;
        public bool MessageNotifyOnCommentReplied { get; set; } = true;
        public bool MessageNotifyOnEditorRecommended { get; set; } = true;
        public bool MessageNotifyOnArticleLiked { get; set; } = true;
        public bool MessageNotifyOnCommentLiked { get; set; } = true;

        public ProfilePoint ProfilePoint { get; set; }
        public ICollection<Point> SubscribedPoints { get; set; }
        public ICollection<NormalPoint> ManagedPoints { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
        
        public ICollection<Message> ReceivedMessages { get; set; }
        public ICollection<UserMessage> SentUserMessages { get; set; }
        public ICollection<OfficialMessageWithSender> SentOfficialMessage { get; set; }

        public ICollection<LoginLog> LoginLogs { get; set; }
        public ICollection<EditLog> EditLogs { get; set; }

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