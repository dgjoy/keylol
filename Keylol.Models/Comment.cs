using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Comment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(30000)]
        public string Content { get; set; }

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        public bool ReadByArticleAuthor { get; set; } = false;

        public bool IgnoredByArticleAuthor { get; set; } = false;

        [Required]
        public string CommentatorId { get; set; }

        public virtual KeylolUser Commentator { get; set; }

        [Required]
        public string ArticleId { get; set; }

        public virtual Article Article { get; set; }

        [Index]
        public int SequenceNumberForArticle { get; set; }

        public bool IgnoreNewLikes { get; set; } = false;

        public bool IgnoreNewComments { get; set; } = false;

        public bool Archived { get; set; } = false;

        public bool Warned { get; set; } = false;

        public virtual ICollection<CommentLike> Likes { get; set; }

        public virtual ICollection<CommentReply> CommentRepliesAsComment { get; set; }

        public virtual ICollection<CommentReply> CommentRepliesAsReply { get; set; }
    }
}