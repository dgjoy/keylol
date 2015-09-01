using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public virtual ProfilePoint Principal { get; set; }

        public virtual ICollection<NormalPoint> AttachedPoints { get; set; }
    }

    public class Article : Entry
    {
        [Required]
        public virtual ArticleType Type { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        public VoteType? Vote { get; set; }

        public bool Muted { get; set; } = false;

        public bool Archived { get; set; } = false;

        public bool GlobalRecommended { get; set; } = false;

        public virtual Article RecommendedArticle { get; set; }
        public virtual ICollection<Article> RecommendedByArticles { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleLike> Likes { get; set; }
        public virtual ICollection<NormalPoint> RecommendedByPoints { get; set; }
        public virtual ICollection<EditLog> EditLogs { get; set; }

        public virtual ICollection<RejectionMessage> RelatedRejectionMessages { get; set; }
        public virtual ICollection<ArticleArchiveMessage> RelatedArchiveMessages { get; set; }
        public virtual ICollection<MuteMessage> RelatedMuteMessages { get; set; }
        public virtual ICollection<RecommendationMessage> RelatedRecommendationMessages { get; set; }
        public virtual ICollection<EditMessage> RelatedEditMessages { get; set; }
        public virtual ICollection<ArticleLikeMessage> RelatedLikeMessages { get; set; }
        public virtual ICollection<ArticleReplyMessage> RelatedReplyMessages { get; set; }
    }

    public class Status : Entry {}
}