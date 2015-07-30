using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

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

    public abstract class SystemMessage : Message {}

    public class SystemMessageWarningNotification : SystemMessage
    {
        [Required]
        public virtual Warning Warning { get; set; }
    }

    public class SystemMessageReplyNotification : SystemMessage
    {
        [Required]
        public virtual Comment Comment { get; set; }
    }

    public class SystemMessageLikeNotification : SystemMessage
    {
        [Required]
        public virtual Like Like { get; set; }
    }
}