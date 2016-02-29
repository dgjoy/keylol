using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class ArticleType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string Description { get; set; } = string.Empty;

        public bool AllowVote { get; set; } = false;

        public virtual ICollection<Article> Articles { get; set; }
    }
}