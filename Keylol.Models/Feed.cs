using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Feed
    {
        public int Id { get; set; }

        [Index]
        [Required]
        [MaxLength(128)]
        public string StreamName { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public FeedEntryType EntryType { get; set; }

        [Index]
        [Required(AllowEmptyStrings = true)]
        [MaxLength(400)]
        public string Entry { get; set; } = string.Empty;

        /// <summary>
        /// JSON 字符串，无 Properties 的 Stream 可以为空，有 Properties 的 Stream 不能为空（但可以为 "[]"）
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string Properties { get; set; } = string.Empty;
    }

    public enum FeedEntryType
    {
        Nothing,
        ArticleId,
        ActivityId,
        ConferenceId,
        ConferenceEntryId,
        PointId,
        UserId
    }
}
