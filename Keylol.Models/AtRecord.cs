using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class AtRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        public AtRecordEntryType EntryType { get; set; }

        [Required]
        public string EntryId { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual KeylolUser User { get; set; }
    }

    public enum AtRecordEntryType
    {
        Article,
        ArticleComment,
        Activity,
        ActivityComment,
        ConferenceEntry
    }
}