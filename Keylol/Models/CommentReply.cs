using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class CommentReply
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CommentId { get; set; }
        public virtual Comment Comment { get; set; }

        public bool ReadByCommentAuthor { get; set; }

        [Required]
        public string ReplyId { get; set; }
        public virtual Comment Reply { get; set; }
    }
}
