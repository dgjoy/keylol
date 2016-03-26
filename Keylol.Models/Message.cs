using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Keylol.Models.DAL;

namespace Keylol.Models
{
    public class Message : IHasSequenceNumber
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

        [Index]
        public bool Unread { get; set; } = true;

        [Index(IsUnique = true)]
        public int SequenceNumber { get; set; }

        public string ArticleId { get; set; }
        public Article Article { get; set; }

        public string CommentId { get; set; }
        public Comment Comment { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        [Index]
        public MessageType Type { get; set; }
    }

    /// <summary>
    /// 认可 0-99
    /// 评论 100-299
    /// 公函 200-399
    /// </summary>
    public enum MessageType
    {
        ArticleLike = 0,
        CommentLike,
        ArticleComment = 100,
        CommentReply,
        ArticleArchive = 200,
        ArticleArchiveCancel,
        CommentArchive,
        CommentArchiveCancel,
        Rejection,
        RejectionCancel,
        Spotlight,
        SpotlightCancel,
        ArticleWarning,
        ArticleWarningCancel,
        CommentWarning,
        CommentWarningCancel
    }

    public static class MessageTypeExtensions
    {
        public static bool IsLikeMessage(this MessageType type)
        {
            return type >= 0 && (int) type <= 99;
        }

        public static bool IsCommentMessage(this MessageType type)
        {
            return (int) type >= 100 && (int) type <= 199;
        }

        public static bool IsMissiveMessage(this MessageType type)
        {
            return (int) type >= 200 && (int) type <= 299;
        }

        public static bool HasArticleProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ArticleLike:
                case MessageType.ArticleArchive:
                case MessageType.ArticleArchiveCancel:
                case MessageType.Rejection:
                case MessageType.RejectionCancel:
                case MessageType.Spotlight:
                case MessageType.SpotlightCancel:
                case MessageType.ArticleWarning:
                case MessageType.ArticleWarningCancel:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasCommentProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.CommentLike:
                case MessageType.ArticleComment:
                case MessageType.CommentReply:
                case MessageType.CommentArchive:
                case MessageType.CommentArchiveCancel:
                case MessageType.CommentWarning:
                case MessageType.CommentWarningCancel:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasReasonProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ArticleArchiveCancel:
                case MessageType.RejectionCancel:
                case MessageType.Spotlight:
                case MessageType.SpotlightCancel:
                case MessageType.ArticleWarningCancel:
                case MessageType.CommentArchiveCancel:
                case MessageType.CommentWarningCancel:
                    return false;

                default:
                    return true;
            }
        }
    }
}