using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class UserGameRecord
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual KeylolUser User { get; set; }

        [Required]
        [Index]
        public int SteamAppId { get; set; }

        public double TotalPlayedTime { get; set; } = 0;

        [Index]
        public DateTime LastPlayTime { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}