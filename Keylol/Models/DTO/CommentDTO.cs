using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class CommentDTO
    {
        public CommentDTO(Comment comment, bool includeContent = true)
        {
            Id = comment.Id;
            if (includeContent)
                Content = comment.Content;
            PublishTime = comment.PublishTime;
            SequenceNumberForArticle = comment.SequenceNumberForArticle;
        }

        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime PublishTime { get; set; }
        public UserDTO Commentotar { get; set; }
        public int SequenceNumberForArticle { get; set; }
        public int? LikeCount { get; set; }
        public bool? Liked { get; set; }
    }
}