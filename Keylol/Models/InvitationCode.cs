using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class InvitationCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime GenerateTime { get; set; } = DateTime.Now;

        public string UserByUserId { get; set; }
        public virtual KeylolUser UsedByUser { get; set; }
    }
}
