using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.DTO
{
    public class ArticleDTO
    {
        public ArticleDTO(Article article, bool includeContent = false, int truncateContentTo = 0)
        {
            Id = article.Id;
            PublishTime = article.PublishTime;
            Title = article.Title;
            if (includeContent)
            {
                Content = truncateContentTo > 0 && truncateContentTo < article.Content.Length
                    ? article.Content.Substring(0, truncateContentTo)
                    : article.Content;
            }
            VoteForPointId = article.VoteForPointId;
            Vote = article.Vote;
            SequenceNumberForAuthor = article.SequenceNumberForAuthor;
            SequenceNumber = article.SequenceNumber;
        }

        public string Id { get; set; }

        public DateTime PublishTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string VoteForPointId { get; set; }

        public VoteType? Vote { get; set; }

        public string AuthorIdCode { get; set; }

        public int SequenceNumberForAuthor { get; set; }

        public int SequenceNumber { get; set; }

        public List<NormalPointDTO> AttachedPoints { get; set; }

        public string TypeName { get; set; }

        public int? LikeCount { get; set; }

        public bool? Liked { get; set; }

        public int? CommentCount { get; set; }

        public SimpleUserDTO Author { get; set; }
    }
}