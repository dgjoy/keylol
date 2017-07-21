using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class SmsValidatingToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        [MaxLength(4)]
        public string Code { get; set; }

        public DateTime SentTime { get; set; }
    }
}
