using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class SteamBotManager
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(64)]
        public string ClientId { get; set; }

        [Required]
        [MaxLength(64)]
        public string ClientSecret { get; set; }

        public virtual ICollection<SteamBot> Bots { get; set; }
    }
}
