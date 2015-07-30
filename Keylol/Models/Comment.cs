using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public class Comment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(30000)]
        public string Content { get; set; }

        public DateTime PublishTime { get; set; }
        public DateTime LastEditTime { get; set; }

        [Required]
        public virtual KeylolUser Commentator { get; set; }

        [Required]
        public virtual Article Article { get; set; }

        public virtual ICollection<CommentLike> Likes { get; set; }
        public virtual ICollection<Comment> ReplyToComments { get; set; }
        public virtual ICollection<Comment> RepliedByComments { get; set; }
    }
}