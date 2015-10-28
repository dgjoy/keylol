using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
