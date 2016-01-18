using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class InvitationCode
    {
        public string Id { get; set; }

        [Index]
        public DateTime GenerateTime { get; set; } = DateTime.Now;

        [Index]
        [Required]
        [MaxLength(64)]
        public string Source { get; set; }

        public virtual KeylolUser UsedByUser { get; set; }
    }
}