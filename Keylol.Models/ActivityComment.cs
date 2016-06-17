using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class ActivityComment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Required]
        [MaxLength(30000)]
        public string Content { get; set; }

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Required]
        public string CommentatorId { get; set; }

        public virtual KeylolUser Commentator { get; set; }

        [Required]
        public string ActivityId { get; set; }

        public virtual Activity Activity { get; set; }

        [Index]
        public int SidForActivity { get; set; }

        public bool DismissLikeMessage { get; set; } = false;

        public bool DismissReplyMessage { get; set; } = false;

        public ArchivedState Archived { get; set; } = ArchivedState.None;

        public bool Warned { get; set; } = false;
    }
}
