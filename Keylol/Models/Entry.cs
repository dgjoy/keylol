using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Entry
    {
        protected Entry()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime PublishTime { get; set; } = DateTime.Now;
        
        [Required]
        public virtual ProfilePoint Principal { get; set; }

        public virtual ICollection<NormalPoint> AttachedPoints { get; set; }
    }

    public class Article : Entry
    {
        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }
        
        public DateTime LastEditTime { get; set; } = DateTime.Now;
        public virtual Article RecommendedArticle { get; set; }
        public virtual ICollection<Article> RecommendedByArticles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleLike> Likes { get; set; }
        
        [Required]
        public virtual ArticleType Type { get; set; }
    }

    public class Status : Entry {}
}