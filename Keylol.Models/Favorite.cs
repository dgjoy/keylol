using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class Favorite
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime AddTime { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }
        public virtual KeylolUser User { get; set; }

        [Required]
        public string PointId { get; set; }
        public virtual Point Point { get; set; }
    }
}
