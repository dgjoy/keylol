using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Sid { get; set; }

        [Index]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        public string ReceiverId { get; set; }

        public virtual KeylolUser Receiver { get; set; }

        [Required]
        public string OperatorId { get; set; }

        public virtual KeylolUser Operator { get; set; }

        [Index]
        public bool Unread { get; set; } = true;

        [Index]
        public MessageType Type { get; set; }

        #region 消息相关属性

        public string ArticleId { get; set; }
        public virtual Article Article { get; set; }

        public string ActivityId { get; set; }
        public virtual Activity Activity { get; set; }

        public string ArticleCommentId { get; set; }
        public virtual ArticleComment ArticleComment { get; set; }

        public string ActivityCommentId { get; set; }
        public virtual ActivityComment ActivityComment { get; set; }

        public int Count { get; set; }

        public int SecondCount { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Reasons { get; set; } = string.Empty;

        #endregion
    }

    /// <summary>
    ///     认可 0-99
    ///     评论 100-199
    ///     公函 200-299
    ///     听众 300-399
    /// </summary>
    public enum MessageType
    {
        ArticleLike = 0,
        ArticleCommentLike,
        ActivityLike,
        ActivityCommentLike,
        ArticleComment = 100,
        ArticleCommentReply,
        ActivityComment,
        ActivityCommentReply,
        ArticleArchive = 200,
        ArticleArchiveCancel,
        ArticleCommentArchive,
        ArticleCommentArchiveCancel,
        ArticleRejection,
        ArticleRejectionCancel,
        Spotlight,
        SpotlightCancel,
        ArticleWarning,
        ArticleWarningCancel,
        ArticleCommentWarning,
        ArticleCommentWarningCancel,
        ActivityArchive,
        ActivityArchiveCancel,
        ActivityCommentArchive,
        ActivityCommentArchiveCancel,
        ActivityRejection,
        ActivityRejectionCancel,
        ActivityWarning,
        ActivityWarningCancel,
        ActivityCommentWarning,
        ActivityCommentWarningCancel,
        NewSubscriber = 300
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

        public static bool IsSubscriberMessage(this MessageType type)
        {
            return (int) type >= 300 && (int) type <= 399;
        }

        public static bool HasArticleProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ArticleLike:
                case MessageType.ArticleArchive:
                case MessageType.ArticleArchiveCancel:
                case MessageType.ArticleRejection:
                case MessageType.ArticleRejectionCancel:
                case MessageType.Spotlight:
                case MessageType.SpotlightCancel:
                case MessageType.ArticleWarning:
                case MessageType.ArticleWarningCancel:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasActivityProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ActivityLike:
                case MessageType.ActivityArchive:
                case MessageType.ActivityArchiveCancel:
                case MessageType.ActivityRejection:
                case MessageType.ActivityRejectionCancel:
                case MessageType.ActivityWarning:
                case MessageType.ActivityWarningCancel:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasArticleCommentProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ArticleCommentLike:
                case MessageType.ArticleComment:
                case MessageType.ArticleCommentReply:
                case MessageType.ArticleCommentArchive:
                case MessageType.ArticleCommentArchiveCancel:
                case MessageType.ArticleCommentWarning:
                case MessageType.ArticleCommentWarningCancel:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasActivityCommentProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ActivityCommentLike:
                case MessageType.ActivityComment:
                case MessageType.ActivityCommentReply:
                case MessageType.ActivityCommentArchive:
                case MessageType.ActivityCommentArchiveCancel:
                case MessageType.ActivityCommentWarning:
                case MessageType.ActivityCommentWarningCancel:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasReasonProperty(this MessageType type)
        {
            switch (type)
            {
                case MessageType.ArticleArchive:
                case MessageType.ArticleRejection:
                case MessageType.ArticleWarning:
                case MessageType.ActivityArchive:
                case MessageType.ActivityRejection:
                case MessageType.ActivityWarning:
                case MessageType.ArticleCommentArchive:
                case MessageType.ArticleCommentWarning:
                case MessageType.ActivityCommentArchive:
                case MessageType.ActivityCommentWarning:
                    return true;

                default:
                    return false;
            }
        }
    }
}