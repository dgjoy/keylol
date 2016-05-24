using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class PointStaff
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Required]
        public string PointId { get; set; }

        public Point Point { get; set; }

        [Required]
        public string StaffId { get; set; }

        public KeylolUser Staff { get; set; }
    }
}