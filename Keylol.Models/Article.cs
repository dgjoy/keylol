using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Keylol.Models.DAL;

namespace Keylol.Models
{
    public class Article : IHaveSequenceNumber
    {
        public string SequenceName { get; } = "ArticleSequence";

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Index(IsUnique = true)]
        public int SequenceNumber { get; set; }

        [Required]
        public string PrincipalId { get; set; }

        public virtual ProfilePoint Principal { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        
        [Index]
        public ArticleType Type { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string ThumbnailImage { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(300000)]
        public string UnstyledContent { get; set; } = string.Empty;

        public int? Vote { get; set; }

        [Index]
        public int SequenceNumberForAuthor { get; set; }

        public bool IgnoreNewLikes { get; set; } = false;

        public bool IgnoreNewComments { get; set; } = false;

        public string VoteForPointId { get; set; }

        public virtual NormalPoint VoteForPoint { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(5000)]
        public string Pros { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(5000)]
        public string Cons { get; set; } = string.Empty;

        public bool Archived { get; set; } = false;

        public bool Rejected { get; set; } = false;

        public bool Spotlight { get; set; } = false;

        public bool Warned { get; set; } = false;

        public virtual ICollection<NormalPoint> AttachedPoints { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<ArticleLike> Likes { get; set; }

        public virtual ICollection<EditLog> EditLogs { get; set; }
    }
}