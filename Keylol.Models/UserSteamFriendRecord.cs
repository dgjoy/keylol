using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class UserSteamFriendRecord
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual KeylolUser User { get; set; }

        [Required]
        [Index]
        [MaxLength(128)]
        public string FriendSteamId { get; set; }

        public DateTime FriendSince { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}