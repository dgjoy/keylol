using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public enum ArticleTypeCategory
    {
        Aging,
        Topic,
        Personal,
        Resource
    }

    public class ArticleType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ArticleTypeCategory Category { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        public bool AllowVote { get; set; } = false;

        public virtual ICollection<Article> Articles { get; set; }
    }
}