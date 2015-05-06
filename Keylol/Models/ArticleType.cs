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
        public ArticleType()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public ArticleTypeCategory Category { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
    }
}