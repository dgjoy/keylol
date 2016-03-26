using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class CommentVM
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public string ArticleId { get; set; }

        [Required]
        public List<int> ReplyToCommentsSN { get; set; }
    }
}