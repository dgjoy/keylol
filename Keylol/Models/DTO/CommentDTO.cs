using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class CommentDTO
    {
        public CommentDTO(Comment comment, bool includeContent = true, int truncateContentTo = 0)
        {
            Id = comment.Id;
            if (includeContent)
            {
                Content = truncateContentTo > 0 && truncateContentTo < comment.Content.Length
                    ? comment.Content.Substring(0, truncateContentTo)
                    : comment.Content;
            }
            PublishTime = comment.PublishTime;
            SequenceNumberForArticle = comment.SequenceNumberForArticle;
        }

        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime PublishTime { get; set; }
        public UserDTO Commentator { get; set; }
        public int SequenceNumberForArticle { get; set; }
        public int? LikeCount { get; set; }
        public bool? Liked { get; set; }

        public ArticleDTO Article { get; set; }
        public UserDTO ReplyToUser { get; set; }
        public CommentDTO ReplyToComment { get; set; }
        public bool? ReplyToMultipleUser { get; set; }
        public bool? Read { get; set; }
    }
}