using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.DTO
{
    public class ArticleDTO
    {
        public enum TimelineReasonType
        {
            Like,
            Publish,
            Point
        }

        public ArticleDTO()
        {
        }

        public ArticleDTO(Article article, bool includeContent = false, int truncateContentTo = 0)
        {
            Id = article.Id;
            PublishTime = article.PublishTime;
            Title = article.Title;
            if (includeContent)
            {
                Content = article.Content;
                TruncateContent(truncateContentTo);
            }
            VoteForPointId = article.VoteForPointId;
            Vote = article.Vote;
            SequenceNumberForAuthor = article.SequenceNumberForAuthor;
            SequenceNumber = article.SequenceNumber;
        }

        public ArticleDTO FlattenAuthor()
        {
            AuthorId = Author.Id;
            AuthorIdCode = Author.IdCode;
            AuthorUserName = Author.UserName;
            AuthorAvatarImage = Author.AvatarImage;
            Author = null;
            return this;
        }

        public ArticleDTO UnflattenAuthor()
        {
            Author = new UserDTO
            {
                Id = AuthorId,
                IdCode = AuthorIdCode,
                UserName = AuthorUserName,
                AvatarImage = AuthorAvatarImage
            };
            AuthorId = null;
            AuthorIdCode = null;
            AuthorUserName = null;
            AuthorAvatarImage = null;
            return this;
        }

        public ArticleDTO TruncateContent(int size)
        {
            if (size > 0 && size < Content.Length)
                Content = Content.Substring(0, size);
            return this;
        }

        public string Id { get; set; }

        public DateTime PublishTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string VoteForPointId { get; set; }

        public string VoteForPointName { get; set; }

        public VoteType? Vote { get; set; }

        public int SequenceNumberForAuthor { get; set; }

        public int SequenceNumber { get; set; }

        public List<NormalPointDTO> AttachedPoints { get; set; }

        public string TypeName { get; set; }

        public int? LikeCount { get; set; }

        public bool? Liked { get; set; }

        public int? CommentCount { get; set; }

        public TimelineReasonType? TimelineReason { get; set; }

        public List<UserDTO> LikeByUsers { get; set; }

        public string AuthorProfilePointBackgroundImage { get; set; }

        #region If Author is not flattened

        public UserDTO Author { get; set; }

        #endregion

        #region If Author is flattened

        public string AuthorId { get; set; }

        public string AuthorIdCode { get; set; }

        public string AuthorUserName { get; set; }

        public string AuthorAvatarImage { get; set; }

        #endregion
    }
}