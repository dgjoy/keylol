using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.DTO
{
    public class ArticleDTO
    {
        public ArticleDTO(Article article, bool includeContent = true)
        {
            Id = article.Id;
            PublishTime = article.PublishTime;
            Title = article.Title;
            if (includeContent)
                Content = article.Content;
            Vote = article.Vote;
            SequenceNumberForAuthor = article.SequenceNumberForAuthor;
        }

        public string Id { get; set; }

        public DateTime PublishTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public VoteType? Vote { get; set; }

        public string AuthorIdCode { get; set; }

        public int SequenceNumberForAuthor { get; set; }
    }
}