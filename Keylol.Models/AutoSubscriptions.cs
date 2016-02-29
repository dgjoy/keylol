using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class AutoSubscriptions
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        public KeylolUser User { get; set; }

        [Required]
        public string NormalPointId { get; set; }

        public virtual NormalPoint NormalPoint { get; set; }
    }
}