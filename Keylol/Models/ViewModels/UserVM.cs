using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class UserPostVM
    {
        [Required]
        public string IdCode { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string AvatarImage { get; set; }

        [Required]
        public string SteamBindingTokenId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string SteamProfileName { get; set; }

        [Required]
        public string GeetestChallenge { get; set; }

        [Required]
        public string GeetestSeccode { get; set; }

        [Required]
        public string GeetestValidate { get; set; }
    }

    public class UserPutVM
    {
        public void CopyToUser(KeylolUser user)
        {
            if (GamerTag != null)
                user.GamerTag = GamerTag;
            if (Email != null)
                user.Email = Email;
            if (AvatarImage != null)
                user.AvatarImage = AvatarImage;
            if (ProfilePointBackgroundImage != null)
                user.ProfilePoint.BackgroundImage = ProfilePointBackgroundImage;

            if (AutoShareOnUploadingScreenshot != null)
                user.AutoShareOnUploadingScreenshot = AutoShareOnUploadingScreenshot.Value;
            if (AutoShareOnAcquiringNewGame != null)
                user.AutoShareOnAcquiringNewGame = AutoShareOnAcquiringNewGame.Value;
            if (AutoShareOnAddingFavorite != null)
                user.AutoShareOnAddingFavorite = AutoShareOnAddingFavorite.Value;
            if (AutoShareOnAddingNewFriend != null)
                user.AutoShareOnAddingNewFriend = AutoShareOnAddingNewFriend.Value;
            if (AutoShareOnAddingVideo != null)
                user.AutoShareOnAddingVideo = AutoShareOnAddingVideo.Value;
            if (AutoShareOnCreatingGroup != null)
                user.AutoShareOnCreatingGroup = AutoShareOnCreatingGroup.Value;
            if (AutoShareOnJoiningGroup != null)
                user.AutoShareOnJoiningGroup = AutoShareOnJoiningGroup.Value;
            if (AutoShareOnPublishingReview != null)
                user.AutoShareOnPublishingReview = AutoShareOnPublishingReview.Value;
            if (AutoShareOnUnlockingAchievement != null)
                user.AutoShareOnUnlockingAchievement = AutoShareOnUnlockingAchievement.Value;
            if (AutoShareOnUpdatingWishlist != null)
                user.AutoShareOnUpdatingWishlist = AutoShareOnUpdatingWishlist.Value;

            // Ignore password

            if (LockoutEnabled != null)
                user.LockoutEnabled = LockoutEnabled.Value;

            if (EmailNotifyOnAdvertisement != null)
                user.EmailNotifyOnAdvertisement = EmailNotifyOnAdvertisement.Value;
            if (EmailNotifyOnArticleReplied != null)
                user.EmailNotifyOnArticleReplied = EmailNotifyOnArticleReplied.Value;
            if (EmailNotifyOnCommentReplied != null)
                user.EmailNotifyOnCommentReplied = EmailNotifyOnCommentReplied.Value;
            if (EmailNotifyOnEditorRecommended != null)
                user.EmailNotifyOnEditorRecommended = EmailNotifyOnEditorRecommended.Value;
            if (EmailNotifyOnMessageReceived != null)
                user.EmailNotifyOnMessageReceived = EmailNotifyOnMessageReceived.Value;

            if (MessageNotifyOnEditorRecommended != null)
                user.MessageNotifyOnEditorRecommended = MessageNotifyOnEditorRecommended.Value;
            if (MessageNotifyOnArticleLiked != null)
                user.MessageNotifyOnArticleLiked = MessageNotifyOnArticleLiked.Value;
            if (MessageNotifyOnArticleReplied != null)
                user.MessageNotifyOnArticleReplied = MessageNotifyOnArticleReplied.Value;
            if (MessageNotifyOnCommentLiked != null)
                user.MessageNotifyOnCommentLiked = MessageNotifyOnCommentLiked.Value;
            if (MessageNotifyOnCommentReplied != null)
                user.MessageNotifyOnCommentReplied = MessageNotifyOnCommentReplied.Value;
        }

        public string GamerTag { get; set; }
        public string Email { get; set; }
        public string AvatarImage { get; set; }
        public string ProfilePointBackgroundImage { get; set; }
        
        public bool? AutoShareOnAddingNewFriend { get; set; }
        public bool? AutoShareOnUnlockingAchievement { get; set; }
        public bool? AutoShareOnAcquiringNewGame { get; set; }
        public bool? AutoShareOnJoiningGroup { get; set; }
        public bool? AutoShareOnCreatingGroup { get; set; }
        public bool? AutoShareOnUpdatingWishlist { get; set; }
        public bool? AutoShareOnPublishingReview { get; set; }
        public bool? AutoShareOnUploadingScreenshot { get; set; }
        public bool? AutoShareOnAddingVideo { get; set; }
        public bool? AutoShareOnAddingFavorite { get; set; }

        public string Password { get; set; }
        public string NewPassword { get; set; }
        public bool? LockoutEnabled { get; set; }
        public string GeetestChallenge { get; set; }
        public string GeetestSeccode { get; set; }
        public string GeetestValidate { get; set; }

        public bool? EmailNotifyOnArticleReplied { get; set; }
        public bool? EmailNotifyOnCommentReplied { get; set; }
        public bool? EmailNotifyOnEditorRecommended { get; set; }
        public bool? EmailNotifyOnMessageReceived { get; set; }
        public bool? EmailNotifyOnAdvertisement { get; set; }
        
        public bool? MessageNotifyOnArticleReplied { get; set; }
        public bool? MessageNotifyOnCommentReplied { get; set; }
        public bool? MessageNotifyOnEditorRecommended { get; set; }
        public bool? MessageNotifyOnArticleLiked { get; set; }
        public bool? MessageNotifyOnCommentLiked { get; set; }
    }
}
