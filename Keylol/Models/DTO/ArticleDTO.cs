using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Keylol.Models.DTO
{
    public class ArticleDTO
    {
        public enum TimelineReasonType
        {
            Like,
            Publish,
            Point,
            AutoSubscription
        }

        public ArticleDTO()
        {
        }

        public ArticleDTO(Article article, bool includeContent = false, int truncateContentTo = 0,
            bool includeThumbnailImage = false, bool includeProsCons = false, bool includeSummary = false)
        {
            Id = article.Id;
            PublishTime = article.PublishTime;
            Title = article.Title;
            if (includeContent)
            {
                Content = truncateContentTo > 0 ? article.UnstyledContent : article.Content;
                TruncateContent(truncateContentTo);
            }
            if (includeThumbnailImage)
            {
                ThumbnailImage = article.ThumbnailImage;
            }
            Vote = article.Vote;
            SequenceNumberForAuthor = article.SequenceNumberForAuthor;
            SequenceNumber = article.SequenceNumber;
            if (includeProsCons)
            {
                Pros = JsonConvert.DeserializeObject<List<string>>(article.Pros);
                Cons = JsonConvert.DeserializeObject<List<string>>(article.Cons);
            }
            if (includeSummary)
            {
                Summary = article.UnstyledContent;
                if (Summary.Length > 200)
                    Summary = Summary.Substring(0, 200);
            }
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

        public ArticleDTO FlattenVoteForPoint()
        {
            VoteForPointId = VoteForPoint.Id;
            VoteForPointPreferredName = VoteForPoint.PreferredName;
            VoteForPointIdCode = VoteForPoint.IdCode;
            VoteForPointChineseName = VoteForPoint.ChineseName;
            VoteForPointEnglishName = VoteForPoint.EnglishName;
            VoteForPoint = null;
            return this;
        }

        public ArticleDTO UnflattenVoteForPoint()
        {
            VoteForPoint = new NormalPointDTO
            {
                Id = VoteForPointId,
                PreferredName = VoteForPointPreferredName ?? PreferredNameType.English,
                IdCode = VoteForPointIdCode,
                ChineseName = VoteForPointChineseName,
                EnglishName = VoteForPointEnglishName
            };
            VoteForPointId = null;
            VoteForPointPreferredName = null;
            VoteForPointIdCode = null;
            VoteForPointChineseName = VoteForPoint.ChineseName;
            VoteForPointEnglishName = VoteForPoint.EnglishName;
            return this;
        }

        public ArticleDTO TruncateContent(int size)
        {
            if (size > 0 && size < Content.Length)
                Content = Content.Substring(0, size);
            return this;
        }

        public string Id { get; set; }

        public DateTime? PublishTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ThumbnailImage { get; set; }

        public int? Vote { get; set; }

        public int? SequenceNumberForAuthor { get; set; }

        public int? SequenceNumber { get; set; }

        public List<NormalPointDTO> AttachedPoints { get; set; }

        public string TypeName { get; set; }

        public int? LikeCount { get; set; }

        public bool? Liked { get; set; }

        public int? CommentCount { get; set; }

        public TimelineReasonType? TimelineReason { get; set; }

        public List<UserDTO> LikeByUsers { get; set; }

        public List<string> Pros { get; set; }

        public List<string> Cons { get; set; }

        public string Summary { get; set; }

        #region If Author is not flattened

        public UserDTO Author { get; set; }

        #endregion

        #region If Author is flattened

        public string AuthorId { get; set; }

        public string AuthorIdCode { get; set; }

        public string AuthorUserName { get; set; }

        public string AuthorAvatarImage { get; set; }

        #endregion

        #region If VoteForPoint is not flattened

        public NormalPointDTO VoteForPoint { get; set; }

        #endregion

        #region If VoteForPoint is flattened

        public string VoteForPointId { get; set; }

        public PreferredNameType? VoteForPointPreferredName { get; set; }

        public string VoteForPointIdCode { get; set; }

        public string VoteForPointChineseName { get; set; }

        public string VoteForPointEnglishName { get; set; }

        #endregion

        internal int? Count { get; set; }
    }
}