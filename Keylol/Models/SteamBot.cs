using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class SteamBot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(64)]
        [Index(IsUnique = true)]
        public string SteamUserName { get; set; }

        [Required]
        [MaxLength(64)]
        public string SteamPassword { get; set; }

        public long? SteamId { get; set; }

        public int FriendCount { get; set; } = 0;

        public int FriendUpperLimit { get; set; } = 50;

        public bool Online { get; set; } = false;

        public virtual ICollection<SteamBindingToken> BindingTokens { get; set; }

        public virtual SteamBotManager Manager { get; set; }

        public virtual ICollection<KeylolUser> Users { get; set; }
    }
}