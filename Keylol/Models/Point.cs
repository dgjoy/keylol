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
        public Point()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public virtual ICollection<KeylolUser> Subscribers { get; set; }
        public virtual ICollection<Article> Articles { get; set; }
        public virtual ICollection<NormalPoint> AssociatedByNormalPoints { get; set; }
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
        public virtual ICollection<Point> AssociatedToPoints { get; set; }
    }

    public class ProfilePoint : Point
    {
        public virtual KeylolUser User { get; set; }
    }
}