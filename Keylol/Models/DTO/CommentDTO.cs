using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class CommentDTO
    {
        public CommentDTO(Comment comment)
        {
            Id = comment.Id;
            Content = comment.Content;
            PublishTime = comment.PublishTime;
            CommentatorId = comment.CommentatorId;
            ArticleId = comment.ArticleId;
        }

        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime PublishTime { get; set; }
        public string CommentatorId { get; set; }
        public string ArticleId { get; set; }
    }
}
