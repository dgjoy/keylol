using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public enum NormalPointType
    {
        Game,
        Genre,
        Manufacturer,
        Platform
    }

    public enum PreferedNameType
    {
        Chinese,
        English
    }

    public abstract class Point
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string BackgroundImage { get; set; } = string.Empty;

        public virtual ICollection<KeylolUser> Subscribers { get; set; }
    }

    public class NormalPoint : Point
    {
        public NormalPointType Type { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(5, MinimumLength = 5)]
        public string IdCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string ChineseName { get; set; }

        [Required]
        [MaxLength(150)]
        public string EnglishName { get; set; }

        public PreferedNameType PreferedName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string Aliases { get; set; } = string.Empty; // Comma separated list

        [Required(AllowEmptyStrings = true)]
        [MaxLength(512)]
        public string StoreLink { get; set; } = string.Empty;

        public virtual ICollection<KeylolUser> Staffs { get; set; }
        public virtual ICollection<NormalPoint> AssociatedToPoints { get; set; }
        public virtual ICollection<NormalPoint> AssociatedByPoints { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }
        public virtual ICollection<Article> RecommendedArticles { get; set; }
        public virtual ICollection<PointRecommendationMessage> RelatedRecommendationMessages { get; set; }
    }

    public class ProfilePoint : Point
    {
        [Required]
        public virtual KeylolUser User { get; set; }

        public ICollection<Entry> Entries { get; set; }
    }
}