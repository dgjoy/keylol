using System;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// Comment DTO
    /// </summary>
    public class CommentDto
    {
        public CommentDto()
        {
        }

        public CommentDto(Comment comment, bool includeContent = true, int truncateContentTo = 0)
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

        /// <summary>
        ///     Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     发表时间
        /// </summary>
        public DateTime PublishTime { get; set; }

        /// <summary>
        ///     评论人
        /// </summary>
        public UserDto Commentator { get; set; }

        /// <summary>
        ///     在文章中的楼层号
        /// </summary>
        public int SequenceNumberForArticle { get; set; }

        /// <summary>
        ///     认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        ///     当前登录用户是否认可过
        /// </summary>
        public bool? Liked { get; set; }

        /// <summary>
        ///     封存状态
        /// </summary>
        public ArchivedState? Archived { get; set; }

        /// <summary>
        ///     警告状态
        /// </summary>
        public bool? Warned { get; set; }
    }
}