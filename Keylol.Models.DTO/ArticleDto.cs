using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     Article DTO
    /// </summary>
    [DataContract]
    public class ArticleDto
    {
        /// <summary>
        ///     时间轴原因
        /// </summary>
        [DataContract]
        public enum TimelineReasonType
        {
            /// <summary>
            ///     被订阅用户认可
            /// </summary>
            [EnumMember] Like,

            /// <summary>
            ///     订阅用户发布
            /// </summary>
            [EnumMember] Publish,

            /// <summary>
            ///     发布到手动订阅的普通据点中
            /// </summary>
            [EnumMember] Point,

            /// <summary>
            ///     发布到同步订阅的据点中
            /// </summary>
            [EnumMember] AutoSubscription
        }

        /// <summary>
        ///     创建空 DTO，需要手动填充
        /// </summary>
        public ArticleDto()
        {
        }

        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        /// <param name="article"><see cref="Article" /> 对象</param>
        /// <param name="includeContent">是否包含文章内容</param>
        /// <param name="truncateContentTo">文章内容截取长度，0 表示不截取</param>
        /// <param name="includeThumbnailImage">是否包含缩略图</param>
        /// <param name="includeProsCons">是否包含优缺点</param>
        /// <param name="includeSummary">是否包含概要</param>
        public ArticleDto(Article article, bool includeContent = false, int truncateContentTo = 0,
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
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     发布时间
        /// </summary>
        [DataMember]
        public DateTime? PublishTime { get; set; }

        /// <summary>
        ///     标题
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        ///     RowVersion
        /// </summary>
        [DataMember]
        public byte[] RowVersion { get; set; }

        /// <summary>
        ///     缩略图
        /// </summary>
        [DataMember]
        public string ThumbnailImage { get; set; }

        /// <summary>
        ///     评分
        /// </summary>
        [DataMember]
        public int? Vote { get; set; }

        /// <summary>
        ///     是作者的第几篇文章
        /// </summary>
        [DataMember]
        public int? SequenceNumberForAuthor { get; set; }

        /// <summary>
        ///     序号
        /// </summary>
        [DataMember]
        public int? SequenceNumber { get; set; }

        /// <summary>
        ///     推送到的据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> AttachedPoints { get; set; }

        /// <summary>
        ///     类型名称
        /// </summary>
        [DataMember]
        public string TypeName { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [DataMember]
        public ArticleType? Type { get; set; }

        /// <summary>
        ///     认可数量
        /// </summary>
        [DataMember]
        public int? LikeCount { get; set; }

        /// <summary>
        ///     当前用户是否认可过
        /// </summary>
        [DataMember]
        public bool? Liked { get; set; }

        /// <summary>
        ///     评论数
        /// </summary>
        [DataMember]
        public int? CommentCount { get; set; }

        /// <summary>
        ///     出现在时间轴的原因
        /// </summary>
        [DataMember]
        public TimelineReasonType? TimelineReason { get; set; }

        /// <summary>
        ///     收到了这些用户的认可
        /// </summary>
        [DataMember]
        public List<UserDto> LikeByUsers { get; set; }

        /// <summary>
        ///     亮点
        /// </summary>
        [DataMember]
        public List<string> Pros { get; set; }

        /// <summary>
        ///     缺点
        /// </summary>
        [DataMember]
        public List<string> Cons { get; set; }

        /// <summary>
        ///     概述总结
        /// </summary>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        ///     封存状态
        /// </summary>
        [DataMember]
        public ArchivedState? Archived { get; set; }

        /// <summary>
        ///     退稿状态
        /// </summary>
        [DataMember]
        public bool? Rejected { get; set; }

        /// <summary>
        ///     萃选状态
        /// </summary>
        [DataMember]
        public bool? Spotlight { get; set; }

        /// <summary>
        ///     警告状态
        /// </summary>
        [DataMember]
        public bool? Warned { get; set; }

        #region If Author is not flattened

        /// <summary>
        ///     作者
        /// </summary>
        [DataMember]
        public UserDto Author { get; set; }

        #endregion

        #region If VoteForPoint is not flattened

        /// <summary>
        ///     评价的据点
        /// </summary>
        [DataMember]
        public NormalPointDto VoteForPoint { get; set; }

        #endregion

        /// <summary>
        ///     计数，仅用于搜索
        /// </summary>
        [DataMember]
        public int? Count { get; set; }

        /// <summary>
        ///     扁平化作者属性
        /// </summary>
        public ArticleDto FlattenAuthor()
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
        public ArticleDto UnflattenAuthor()
        {
            Author = new UserDto
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
        public ArticleDto FlattenVoteForPoint()
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
        public ArticleDto UnflattenVoteForPoint()
        {
            VoteForPoint = new NormalPointDto
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
        public ArticleDto TruncateContent(int size)
        {
            if (size > 0 && size < Content.Length)
                Content = Content.Substring(0, size);
            return this;
        }

        #region If Author is flattened

        /// <summary>
        ///     作者 Id
        /// </summary>
        [DataMember]
        public string AuthorId { get; set; }

        /// <summary>
        ///     作者识别码
        /// </summary>
        [DataMember]
        public string AuthorIdCode { get; set; }

        /// <summary>
        ///     作者用户名
        /// </summary>
        [DataMember]
        public string AuthorUserName { get; set; }

        /// <summary>
        ///     作者头像
        /// </summary>
        [DataMember]
        public string AuthorAvatarImage { get; set; }

        #endregion

        #region If VoteForPoint is flattened

        /// <summary>
        ///     评价据点 Id
        /// </summary>
        [DataMember]
        public string VoteForPointId { get; set; }

        /// <summary>
        ///     评价据点主显名称偏好
        /// </summary>
        [DataMember]
        public PreferredNameType? VoteForPointPreferredName { get; set; }

        /// <summary>
        ///     评价据点识别码
        /// </summary>
        [DataMember]
        public string VoteForPointIdCode { get; set; }

        /// <summary>
        ///     评价据点中文名
        /// </summary>
        [DataMember]
        public string VoteForPointChineseName { get; set; }

        /// <summary>
        ///     评价据点英文名
        /// </summary>
        [DataMember]
        public string VoteForPointEnglishName { get; set; }

        #endregion
    }
}