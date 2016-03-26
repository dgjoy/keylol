using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public class CommentReply
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 被回复的评论
        /// </summary>
        [Required]
        public string CommentId { get; set; }

        public virtual Comment Comment { get; set; }

        /// <summary>
        /// 新回复的评论
        /// </summary>
        [Required]
        public string ReplyId { get; set; }

        public virtual Comment Reply { get; set; }
    }
}