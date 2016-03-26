using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using SteamKit2;

namespace Keylol.Models.DTO
{
    [DataContract]
    public class UserDTO
    {
        private readonly KeylolUser _user;

        public UserDTO()
        {
        }

        public UserDTO(KeylolUser user, bool includeSteam = false, bool includeSecurity = false)
        {
            _user = user;
            Id = user.Id;
            IdCode = user.IdCode;
            UserName = user.UserName;
            GamerTag = user.GamerTag;

            // Ignore ProfilePointBackgroundImage

            AvatarImage = user.AvatarImage;

            if (includeSteam)
                IncludeSteam();

            if (includeSecurity)
                IncludeSecurity();

            // Ignore claims

            // Ignore SteamBot

            // Ignore stats

            // Ignore subscribed
        }

        public UserDTO IncludeSecurity()
        {
            LockoutEnabled = _user.LockoutEnabled;
            Email = _user.Email;
            return this;
        }

        public UserDTO IncludeSteam()
        {
            SteamId = _user.SteamId;
            var steamId = new SteamID();
            steamId.SetFromSteam3String(SteamId);
            SteamId64 = steamId.ConvertToUInt64().ToString();
            SteamProfileName = _user.SteamProfileName;
            return this;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string IdCode { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string GamerTag { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string AvatarImage { get; set; }

        [DataMember]
        public string ProfilePointBackgroundImage { get; set; }

        [DataMember]
        public bool? LockoutEnabled { get; set; }

        [DataMember]
        public string SteamId { get; set; }

        [DataMember]
        public string SteamId64 { get; set; }

        [DataMember]
        public string SteamProfileName { get; set; }

        [DataMember]
        public string StatusClaim { get; set; }

        [DataMember]
        public string StaffClaim { get; set; }

        [DataMember]
        public SteamBotDTO SteamBot { get; set; }

        [DataMember]
        public int? SubscribedPointCount { get; set; }

        [DataMember]
        public int? SubscriberCount { get; set; }

        [DataMember]
        public int? ArticleCount { get; set; }

        [DataMember]
        public bool? Subscribed { get; set; }

        [DataMember]
        public string MessageCount { get; set; }

        [DataMember]
        public int? ReviewCount { get; set; }

        [DataMember]
        public int? ShortReviewCount { get; set; }
    }

    public class UserWithMoreOptionsDTO : UserDTO
    {
        public UserWithMoreOptionsDTO(KeylolUser user) : base(user)
        {
            SteamNotifyOnArticleReplied = user.SteamNotifyOnArticleReplied;
            SteamNotifyOnCommentReplied = user.SteamNotifyOnCommentReplied;
            SteamNotifyOnArticleLiked = user.SteamNotifyOnArticleLiked;
            SteamNotifyOnCommentLiked = user.SteamNotifyOnCommentLiked;

            AutoSubscribeEnabled = user.AutoSubscribeEnabled;
            AutoSubscribeDaySpan = user.AutoSubscribeDaySpan;
        }
        
        [DataMember]
        public bool SteamNotifyOnArticleReplied { get; set; }

        [DataMember]
        public bool SteamNotifyOnCommentReplied { get; set; }

        [DataMember]
        public bool SteamNotifyOnArticleLiked { get; set; }

        [DataMember]
        public bool SteamNotifyOnCommentLiked { get; set; }

        [DataMember]
        public bool AutoSubscribeEnabled { get; set; }

        [DataMember]
        public double AutoSubscribeDaySpan { get; set; }
    }
}