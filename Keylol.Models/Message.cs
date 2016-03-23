using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public abstract class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime CreateTime { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public KeylolUser Receiver { get; set; }
    }

    public class LikeMessage : Message
    {
    }

    public class ArticleLikeMessage : Message
    {
        public string ArticleId { get; set; }
        public Article Article { get; set; }
    }

    public class CommentLikeMessage : LikeMessage
    {
        public string CommentId { get; set; }
        public Comment Comment { get; set; }
    }

    public abstract class CommentMessage : Message
    {
    }

    public class ArticleCommentMessage : CommentMessage
    {
        public string ArticleId { get; set; }
        public Article Article { get; set; }
    }

    public class CommentReplyMessage : CommentMessage
    {
        public string CommentId { get; set; }
        public Comment Comment { get; set; }
    }

    public class MissiveMessage : Message
    {
    }

    public class ArchiveMissiveMessage : MissiveMessage
    {
    }

    public class ArchiveCancelMissiveMessage : MissiveMessage
    {
    }

    public class RejectionMissiveMessage : MissiveMessage
    {
    }

    public class RejectionCancelMissiveMessage : MissiveMessage
    {
    }

    public class SpotlightMissiveMessage : MissiveMessage
    {
    }

    public class SpotlightCancelMissiveMessage : MissiveMessage
    {
    }

    public class ArticleWarningMissiveMessage : MissiveMessage
    {
    }

    public class ArticleWarningCancelMissiveMessage : MissiveMessage
    {
    }

    public class CommentWarningMissiveMessage : MissiveMessage
    {
    }

    public class CommentWarningCancelMissiveMessage : MissiveMessage
    {
    }
}