using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Article
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

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

        [Index]
        public ArchivedState Archived { get; set; } = ArchivedState.None;

        [Index]
        public bool Rejected { get; set; } = false;

        [Index]
        public DateTime? SpotlightTime { get; set; }

        public bool Warned { get; set; } = false;

        public virtual ICollection<NormalPoint> AttachedPoints { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<ArticleLike> Likes { get; set; }

        public virtual ICollection<EditLog> EditLogs { get; set; }

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int SequenceNumber { get; set; }
    }

    public enum ArticleType
    {
        简评,
        评,
        研,
        谈,
        讯,
        档
    }

    public static class ArticleTypeExtensions
    {
        public static bool AllowVote(this ArticleType type)
        {
            return type == ArticleType.简评 || type == ArticleType.评;
        }
    }

    public enum ArchivedState
    {
        /// <summary>
        ///     没有封存
        /// </summary>
        None,

        /// <summary>
        ///     用户自行封存
        /// </summary>
        User,

        /// <summary>
        ///     运维职员封存
        /// </summary>
        Operator
    }
}