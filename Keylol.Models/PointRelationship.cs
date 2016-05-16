using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class PointRelationship
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        public PointRelationshipType Relationship { get; set; }

        /// <summary>
        /// 必须是游戏、硬件据点
        /// </summary>
        [Required]
        public string SourcePointId { get; set; }

        public virtual Point SourcePoint { get; set; }

        [Required]
        public string TargetPointId { get; set; }

        public virtual Point TargetPoint { get; set; }
    }

    public enum PointRelationshipType
    {
        Developer,
        Publisher,
        Manufacturer,
        Genre,
        Series,
        Tag,
        Platform,
        Reseller
    }
}