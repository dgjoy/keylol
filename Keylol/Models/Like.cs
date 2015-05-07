using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Like
    {
        public Like()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public DateTime Time { get; set; }

        [Required]
        public virtual KeylolUser Operator { get; set; }
    }

    public class ArticleLike : Like
    {
        [Required]
        public virtual Article Article { get; set; }
    }

    public class CommentLike : Like
    {
        [Required]
        public virtual Comment Comment { get; set; }
    }
}