using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public class PointStaff
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PointId { get; set; }

        public Point Point { get; set; }

        [Required]
        public string StaffId { get; set; }

        public KeylolUser Staff { get; set; }
    }
}