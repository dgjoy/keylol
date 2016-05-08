using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Reply
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        public ReplyEntryType EntryType { get; set; }

        /// <summary>
        ///     被回复的对象
        /// </summary>
        [Required]
        [Index]
        [MaxLength(128)]
        public string EntryId { get; set; }

        /// <summary>
        ///     新回复的对象
        /// </summary>
        [Required]
        [Index]
        [MaxLength(128)]
        public string ReplyId { get; set; }
    }

    public enum ReplyEntryType
    {
        ArticleComment,
        ActivityComment,
        ConferenceEntry
    }
}