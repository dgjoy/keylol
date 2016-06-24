using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class SteamStoreName
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Sid { get; set; }

        [Required]
        [MaxLength(256)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<Point> Points { get; set; }
    }
}