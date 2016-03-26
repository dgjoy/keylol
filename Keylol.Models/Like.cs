using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public abstract class Like
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime Time { get; set; } = DateTime.Now;
        
        [Required]
        public string OperatorId { get; set; }

        public virtual KeylolUser Operator { get; set; }
    }

    public class ArticleLike : Like
    {
        [Required]
        public string ArticleId { get; set; }

        public virtual Article Article { get; set; }
    }

    public class CommentLike : Like
    {
        [Required]
        public string CommentId { get; set; }

        public virtual Comment Comment { get; set; }
    }
}