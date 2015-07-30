using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Piece
    {
        protected Piece()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Required]
        public virtual Point Principal { get; set; }

        public virtual ICollection<Point> AttachedPoints { get; set; }
    }

    public class Article : Piece
    {
        public Article()
        {
            LastEditTime = DateTime.Now;
        }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        public DateTime LastEditTime { get; set; }
        public virtual Article RecommendedArticle { get; set; }
        public virtual ICollection<Article> RecommendedByArticles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleLike> Likes { get; set; }

        [Required]
        public virtual ArticleType Type { get; set; }
    }

    public class Status : Piece {}
}