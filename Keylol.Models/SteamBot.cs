using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class SteamBot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Required]
        [MaxLength(64)]
        [Index(IsUnique = true)]
        public string SteamUserName { get; set; }

        [Required]
        [MaxLength(64)]
        public string SteamPassword { get; set; }

        [MaxLength(64)]
        [Index]
        public string SteamId { get; set; }

        [Index]
        public int FriendCount { get; set; } = 0;

        public int FriendUpperLimit { get; set; } = 50;

        [Index]
        public bool Online { get; set; } = false;

        [MaxLength(128)]
        [Index]
        public string SessionId { get; set; }

        [Index]
        public bool Enabled { get; set; } = true;
    }
}