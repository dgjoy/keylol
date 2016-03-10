using System;
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

            // Ignore password

            if (LockoutEnabled != null)
                user.LockoutEnabled = LockoutEnabled.Value;

            if (SteamNotifyOnArticleReplied != null)
                user.SteamNotifyOnArticleReplied = SteamNotifyOnArticleReplied.Value;
            if (SteamNotifyOnArticleLiked != null)
                user.SteamNotifyOnArticleLiked = SteamNotifyOnArticleLiked.Value;
            if (SteamNotifyOnCommentReplied != null)
                user.SteamNotifyOnCommentReplied = SteamNotifyOnCommentReplied.Value;
            if (SteamNotifyOnCommentLiked != null)
                user.SteamNotifyOnCommentLiked = SteamNotifyOnCommentLiked.Value;

            if (AutoSubscribeEnabled != null)
                user.AutoSubscribeEnabled = AutoSubscribeEnabled.Value;
            if (AutoSubscribeDaySpan != null)
                user.AutoSubscribeDaySpan = AutoSubscribeDaySpan.Value;
        }

        public string GamerTag { get; set; }
        public string Email { get; set; }
        public string AvatarImage { get; set; }
        public string ProfilePointBackgroundImage { get; set; }

        public string Password { get; set; }
        public string NewPassword { get; set; }
        public bool? LockoutEnabled { get; set; }
        public string GeetestChallenge { get; set; }
        public string GeetestSeccode { get; set; }
        public string GeetestValidate { get; set; }

        public bool? SteamNotifyOnArticleReplied { get; set; }
        public bool? SteamNotifyOnCommentReplied { get; set; }
        public bool? SteamNotifyOnArticleLiked { get; set; }
        public bool? SteamNotifyOnCommentLiked { get; set; }

        public bool? AutoSubscribeEnabled { get; set; }
        public int? AutoSubscribeDaySpan { get; set; }
    }
}
