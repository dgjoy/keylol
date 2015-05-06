using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public class Article
    {
        public Article()
        {
            Id = Guid.NewGuid().ToString();
            PublishTime = DateTime.Now;
            LastEditTime = DateTime.Now;
        }

        public string Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        public DateTime PublishTime { get; set; }
        public DateTime LastEditTime { get; set; }
        public virtual ICollection<Point> Points { get; set; }
        public virtual Article RecommendedArticle { get; set; }
        public virtual ICollection<Article> RecommendedByArticles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleLike> Likes { get; set; }

        [Required]
        public virtual ArticleType Type { get; set; }
    }
}