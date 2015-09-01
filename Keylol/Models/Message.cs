using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Time { get; set; } = DateTime.Now;
        public bool Read { get; set; } = false;

        [Required]
        public virtual KeylolUser Receiver { get; set; }
    }

    public class UserMessage : Message
    {
        [Required]
        [MaxLength(100000)]
        public string Content { get; set; }

        [Required]
        public virtual KeylolUser Sender { get; set; }
    }

    public abstract class OfficialMessage : Message
    {
    }

    public abstract class OfficialMessageWithSender : OfficialMessage
    {
        [Required]
        public virtual KeylolUser Sender { get; set; }
    }

    public abstract class CorrectionalServiceMessage : OfficialMessageWithSender
    {
    }

    public abstract class EditingMessage : OfficialMessageWithSender
    {
    }

    public abstract class SocialMessage : OfficialMessage
    {
    }

    public abstract class SystemMessage : OfficialMessage
    {
    }

    public class WarningMessage : CorrectionalServiceMessage
    {
    }

    public class RejectionMessage : CorrectionalServiceMessage
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public enum ArchiveType
    {
        Archive,
        Unarchive
    }

    public abstract class ArchiveMessage : CorrectionalServiceMessage
    {
        public ArchiveType Type { get; set; }
    }

    public class ArticleArchiveMessage : ArchiveMessage
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public class CommentArchiveMessage : ArchiveMessage
    {
        [Required]
        public virtual Comment Comment { get; set; }
    }

    public enum MuteType
    {
        Mute,
        Unmute
    }

    public class MuteMessage : CorrectionalServiceMessage
    {
        public MuteType Type { get; set; }

        [Required]
        public virtual Article Article { get; set; }
    }

    public abstract class RecommendationMessage : EditingMessage
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public class PointRecommendationMessage : RecommendationMessage
    {
        [Required]
        public virtual NormalPoint Point { get; set; }
    }

    public class GlobalRecommendationMessage : RecommendationMessage
    {
    }

    public class EditMessage : EditingMessage
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public abstract class LikeMessage : SocialMessage
    {
        public virtual ICollection<Like> Likes { get; set; }
    }

    public class ArticleLikeMessage : LikeMessage
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public class CommentLikeMessage : LikeMessage
    {
        [Required]
        public virtual Comment Comment { get; set; }
    }

    public abstract class ReplyMessage : SocialMessage
    {
        [Required]
        public virtual Comment Comment { get; set; }
    }

    public class ArticleReplyMessage : ReplyMessage
    {
        [Required]
        public virtual Article Target { get; set; }
    }

    public class CommentReplyMessage : ReplyMessage
    {
        [Required]
        public virtual Comment Target { get; set; }
    }

    public class AnnouncementMessage : SystemMessage
    {
        [Required]
        [MaxLength(100000)]
        public string Content { get; set; }
    }

    public class AdvertisementMessage : SystemMessage
    {
    }
}