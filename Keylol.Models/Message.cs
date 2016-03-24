using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Keylol.Models.DAL;

namespace Keylol.Models
{
    public abstract class Message : IHaveSequenceNumber
    {
        public string SequenceName { get; } = "MessageSequence";

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        public string ReceiverId { get; set; }

        public KeylolUser Receiver { get; set; }

        [Required]
        public string OperatorId { get; set; }

        public KeylolUser Operator { get; set; }

        public bool Unread { get; set; } = true;

        [Index(IsUnique = true)]
        public int SequenceNumber { get; set; }
    }

    public class LikeMessage : Message
    {
    }

    public class ArticleLikeMessage : Message
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class CommentLikeMessage : LikeMessage
    {
        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }

    public abstract class CommentMessage : Message
    {
    }

    public class ArticleCommentMessage : CommentMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class CommentReplyMessage : CommentMessage
    {
        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }

    public class MissiveMessage : Message
    {
    }

    public class ArticleArchiveMissiveMessage : MissiveMessage
    {
        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class ArticleArchiveCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class CommentArchiveMissiveMessage : MissiveMessage
    {
        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }

    public class CommentArchiveCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }

    public class RejectionMissiveMessage : MissiveMessage
    {
        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class RejectionCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class SpotlightMissiveMessage : MissiveMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class SpotlightCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class ArticleWarningMissiveMessage : MissiveMessage
    {
        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class ArticleWarningCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string ArticleId { get; set; }

        public Article Article { get; set; }
    }

    public class CommentWarningMissiveMessage : MissiveMessage
    {
        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }

    public class CommentWarningCancelMissiveMessage : MissiveMessage
    {
        [Required]
        public string CommentId { get; set; }

        public Comment Comment { get; set; }
    }
}