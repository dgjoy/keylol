using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Keylol.Models.ViewModels
{
    [DataContract]
    public class SteamBotVM
    {
        [DataMember]
        [Required]
        public string Id { get; set; }

        [DataMember]
        public string SteamId { get; set; }

        [DataMember]
        public int? FriendCount { get; set; }

        [DataMember]
        public bool? Online { get; set; }
    }
}