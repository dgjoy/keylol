using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class ArticleVM
    {
        [Required]
        public string TypeId { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        [Required]
        public List<string> AttachedPointsId { get; set; }

        public string VoteForPointId { get; set; }

        public VoteType? Vote { get; set; }
    }
}
