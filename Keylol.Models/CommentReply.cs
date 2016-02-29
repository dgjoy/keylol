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

        public bool IgnoredByCommentAuthor { get; set; } = false;

        public bool ReadByCommentAuthor { get; set; } = false;

        /// <summary>
        /// 新回复的评论
        /// </summary>
        [Required]
        public string ReplyId { get; set; }

        public virtual Comment Reply { get; set; }
    }
}