using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Like
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Sid { get; set; }

        [Index]
        public DateTime Time { get; set; } = DateTime.Now;

        [Required]
        public string OperatorId { get; set; }

        public virtual KeylolUser Operator { get; set; }

        public LikeTargetType TargetType { get; set; }

        [Index]
        [Required]
        [MaxLength(128)]
        public string TargetId { get; set; }
    }

    public enum LikeTargetType
    {
        Article,
        ArticleComment,
        Activity,
        ActivityComment,
        ConferenceEntry
    }
}