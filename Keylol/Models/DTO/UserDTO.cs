using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.DTO
{
    public class UserDTO
    {
        public UserDTO(KeylolUser user, bool includeProfilePointBackgroundImage = false)
        {
            Id = user.Id;
            IdCode = user.IdCode;
            UserName = user.UserName;
            GamerTag = user.GamerTag;
            Email = user.Email;
            AvatarImage = user.AvatarImage;
            LockoutEnabled = user.LockoutEnabled;

            if (includeProfilePointBackgroundImage)
                ProfilePointBackgroundImage = user.ProfilePoint.BackgroundImage;

            AutoShareOnAcquiringNewGame = user.AutoShareOnAcquiringNewGame;
            AutoShareOnAddingFavorite = user.AutoShareOnAddingFavorite;
            AutoShareOnAddingNewFriend = user.AutoShareOnAddingNewFriend;
            AutoShareOnAddingVideo = user.AutoShareOnAddingVideo;
            AutoShareOnCreatingGroup = user.AutoShareOnCreatingGroup;
            AutoShareOnJoiningGroup = user.AutoShareOnJoiningGroup;
            AutoShareOnPublishingReview = user.AutoShareOnPublishingReview;
            AutoShareOnUnlockingAchievement = user.AutoShareOnUnlockingAchievement;
            AutoShareOnUpdatingWishlist = user.AutoShareOnUpdatingWishlist;
            AutoShareOnUploadingScreenshot = user.AutoShareOnUploadingScreenshot;

            EmailNotifyOnAdvertisement = user.EmailNotifyOnAdvertisement;
            EmailNotifyOnArticleReplied = user.EmailNotifyOnArticleReplied;
            EmailNotifyOnCommentReplied = user.EmailNotifyOnCommentReplied;
            EmailNotifyOnEditorRecommended = user.EmailNotifyOnEditorRecommended;
            EmailNotifyOnMessageReceived = user.EmailNotifyOnMessageReceived;

            MessageNotifyOnArticleLiked = user.MessageNotifyOnArticleLiked;
            MessageNotifyOnArticleReplied = user.MessageNotifyOnArticleReplied;
            MessageNotifyOnCommentLiked = user.MessageNotifyOnCommentLiked;
            MessageNotifyOnCommentReplied = user.MessageNotifyOnCommentReplied;
            MessageNotifyOnEditorRecommended = user.MessageNotifyOnEditorRecommended;
        }

        public string Id { get; set; }
        public string IdCode { get; set; }
        public string UserName { get; set; }
        public string GamerTag { get; set; }
        public string Email { get; set; }
        public string AvatarImage { get; set; }
        public string ProfilePointBackgroundImage { get; set; }
        public bool LockoutEnabled { get; set; }

        public bool AutoShareOnAddingNewFriend { get; set; }
        public bool AutoShareOnUnlockingAchievement { get; set; }
        public bool AutoShareOnAcquiringNewGame { get; set; }
        public bool AutoShareOnJoiningGroup { get; set; }
        public bool AutoShareOnCreatingGroup { get; set; }
        public bool AutoShareOnUpdatingWishlist { get; set; }
        public bool AutoShareOnPublishingReview { get; set; }
        public bool AutoShareOnUploadingScreenshot { get; set; }
        public bool AutoShareOnAddingVideo { get; set; }
        public bool AutoShareOnAddingFavorite { get; set; }

        public bool EmailNotifyOnArticleReplied { get; set; }
        public bool EmailNotifyOnCommentReplied { get; set; }
        public bool EmailNotifyOnEditorRecommended { get; set; }
        public bool EmailNotifyOnMessageReceived { get; set; }
        public bool EmailNotifyOnAdvertisement { get; set; }

        public bool MessageNotifyOnArticleReplied { get; set; }
        public bool MessageNotifyOnCommentReplied { get; set; }
        public bool MessageNotifyOnEditorRecommended { get; set; }
        public bool MessageNotifyOnArticleLiked { get; set; }
        public bool MessageNotifyOnCommentLiked { get; set; }
    }
}
