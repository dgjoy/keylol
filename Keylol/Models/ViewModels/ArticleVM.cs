using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class ArticlePostVM
    {
        [Required]
        public string TypeId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public List<string> AttachedPointsId { get; set; }

        public string VoteForPointId { get; set; }

        public VoteType? Vote { get; set; }
    }

    public class ArticlePutVM
    {
        public string TypeId { get; set; }
        
        public string Title { get; set; }
        
        public string Content { get; set; }
        
        public List<string> AttachedPointsId { get; set; }

        public string VoteForPointId { get; set; }

        public VoteType? Vote { get; set; }
    }
}
