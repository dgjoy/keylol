using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.ViewModels
{
    [DataContract(Namespace = "http://xmlns.keylol.com/wcf/2015/09")]
    public class SteamBotVM
    {
        [DataMember]
        [Required]
        public string Id { get; set; }
        
        [DataMember]
        public long? SteamId { get; set; }

        [DataMember]
        public int? FriendCount { get; set; }

        [DataMember]
        public bool? Online { get; set; }
    }
}
