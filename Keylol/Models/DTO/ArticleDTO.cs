using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     文章 DTO
    /// </summary>
    public class ArticleDTO
    {
        /// <summary>
        ///     时间轴原因
        /// </summary>
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

        /// <summary>
        ///     Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     发表时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        ///     标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

        /// <summary>
        ///     评分
        /// </summary>
        public int? Vote { get; set; }

        /// <summary>
        ///     是作者的第几篇文章
        /// </summary>
        public int? SequenceNumberForAuthor { get; set; }

        /// <summary>
        ///     序号
        /// </summary>
        public int? SequenceNumber { get; set; }

        /// <summary>
        ///     推送到的据点
        /// </summary>
        public List<NormalPointDTO> AttachedPoints { get; set; }

        /// <summary>
        ///     类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        public ArticleType? Type { get; set; }

        /// <summary>
        ///     认可数量
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        ///     当前用户是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        ///     评论数
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        ///     出现在时间轴的原因
        /// </summary>
        public TimelineReasonType? TimelineReason { get; set; }

        /// <summary>
        ///     收到了这些用户的认可
        /// </summary>
        public List<UserDTO> LikeByUsers { get; set; }

        /// <summary>
        ///     亮点
        /// </summary>
        public List<string> Pros { get; set; }

        /// <summary>
        ///     缺点
        /// </summary>
        public List<string> Cons { get; set; }

        /// <summary>
        ///     概述总结
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///     封存状态
        /// </summary>
        public ArchivedState? Archived { get; set; }

        /// <summary>
        ///     退稿状态
        /// </summary>
        public bool? Rejected { get; set; }

        /// <summary>
        ///     萃选状态
        /// </summary>
        public bool? Spotlight { get; set; }

        /// <summary>
        ///     警告状态
        /// </summary>
        public bool? Warned { get; set; }

        #region If Author is not flattened

        public UserDTO Author { get; set; }

        #endregion

        #region If VoteForPoint is not flattened

        public NormalPointDTO VoteForPoint { get; set; }

        #endregion

        internal int? Count { get; set; }

        /// <summary>
        ///     扁平化作者属性
        /// </summary>
        public ArticleDTO FlattenAuthor()
        {
            AuthorId = Author.Id;
            AuthorIdCode = Author.IdCode;
            AuthorUserName = Author.UserName;
            AuthorAvatarImage = Author.AvatarImage;
            Author = null;
            return this;
        }

        /// <summary>
        ///     反扁平化作者属性
        /// </summary>
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

        /// <summary>
        ///     扁平化评价据点的属性
        /// </summary>
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

        /// <summary>
        ///     反扁平化评价据点的属性
        /// </summary>
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

        /// <summary>
        ///     缩短内容
        /// </summary>
        /// <param name="size">要保留的大小</param>
        public ArticleDTO TruncateContent(int size)
        {
            if (size > 0 && size < Content.Length)
                Content = Content.Substring(0, size);
            return this;
        }

        #region If Author is flattened

        public string AuthorId { get; set; }

        public string AuthorIdCode { get; set; }

        public string AuthorUserName { get; set; }

        public string AuthorAvatarImage { get; set; }

        #endregion

        #region If VoteForPoint is flattened

        public string VoteForPointId { get; set; }

        public PreferredNameType? VoteForPointPreferredName { get; set; }

        public string VoteForPointIdCode { get; set; }

        public string VoteForPointChineseName { get; set; }

        public string VoteForPointEnglishName { get; set; }

        #endregion
    }
}