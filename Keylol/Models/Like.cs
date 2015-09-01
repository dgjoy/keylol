using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Like
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Time { get; set; } = DateTime.Now;
        
        [Required]
        public virtual KeylolUser Operator { get; set; }

        public virtual ICollection<LikeMessage> RelatedLikeMessages { get; set; }
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