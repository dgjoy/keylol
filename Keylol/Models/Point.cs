using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public enum NormalPointType
    {
        Game,
        Genre,
        Manufacturer,
        Platform
    }

    public abstract class Point
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public virtual ICollection<KeylolUser> Subscribers { get; set; }
    }

    public class NormalPoint : Point
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string AlternativeName { get; set; }

        [Required]
        [MaxLength(100)]
        public string UrlFriendlyName { get; set; }

        public NormalPointType Type { get; set; }
        public virtual ICollection<NormalPoint> AssociatedToPoints { get; set; }
        public virtual ICollection<NormalPoint> AssociatedByPoints { get; set; }
        public virtual ICollection<KeylolUser> Staffs { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }
    }

    public class ProfilePoint : Point
    {
        [Required]
        public virtual KeylolUser User { get; set; }

        public ICollection<Entry> Entries => null;
    }
}