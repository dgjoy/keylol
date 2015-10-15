using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public enum VoteType
    {
        Positive,
        Negative
    }

    public abstract class Entry
    {
        protected Entry()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime PublishTime { get; set; } = DateTime.Now;
        
        [Required]
        public string PrincipalId { get; set; }

        public virtual ProfilePoint Principal { get; set; }
    }

    public class Article : Entry
    {
        [Required]
        public string TypeId { get; set; }
        public virtual ArticleType Type { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        public VoteType? Vote { get; set; }
        
        [Index]
        public int SequenceNumberForAuthor { get; set; }
        
        public virtual ICollection<NormalPoint> AttachedPoints { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleLike> Likes { get; set; }
        public virtual ICollection<EditLog> EditLogs { get; set; }
        public string VoteForPointId { get; set; }
        public virtual NormalPoint VoteForPoint { get; set; }
    }

    public class Status : Entry {}
}