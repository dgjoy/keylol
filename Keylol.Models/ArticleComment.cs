using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class ArticleComment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Sid { get; set; }

        [Required]
        [MaxLength(30000)]
        public string Content { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(30000)]
        public string UnstyledContent { get; set; } = string.Empty;

        public string ReplyToCommentId { get; set; }

        public virtual ArticleComment ReplyToComment { get; set; }

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Required]
        public string CommentatorId { get; set; }

        public virtual KeylolUser Commentator { get; set; }

        [Required]
        public string ArticleId { get; set; }

        public virtual Article Article { get; set; }

        [Index]
        public int SidForArticle { get; set; }
        
        public bool DismissLikeMessage { get; set; } = false;

        public bool DismissReplyMessage { get; set; } = false;

        public ArchivedState Archived { get; set; } = ArchivedState.None;

        public bool Warned { get; set; } = false;
    }
}