using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public class SteamBot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(64)]
        public string SteamUserName { get; set; }

        [Required]
        [MaxLength(64)]
        public string SteamPassword { get; set; }

        public long? SteamId { get; set; }

        public int FriendCount { get; set; } = 0;

        public int FriendUpperLimit { get; set; } = 50;
    }
}