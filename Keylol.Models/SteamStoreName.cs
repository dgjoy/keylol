using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class SteamStoreName
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(256)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<NormalPoint> NormalPoints { get; set; }
    }
}
