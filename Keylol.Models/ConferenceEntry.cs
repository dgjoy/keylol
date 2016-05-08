using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class ConferenceEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Required]
        public string AuthorId { get; set; }

        public virtual KeylolUser Author { get; set; }

        [Index]
        public int SidForAuthor { get; set; }

        [Required]
        public string ConferenceId { get; set; }

        public virtual Conference Conference { get; set; }

        [Index]
        public int SidForConference { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string ThumbnailImage { get; set; } = string.Empty;
    }
}
