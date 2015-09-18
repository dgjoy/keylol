using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    [DataContract(Namespace = "http://xmlns.keylol.com/wcf/2015/09")]
    public class SteamBotDTO
    {
        public SteamBotDTO(SteamBot bot)
        {
            Id = bot.Id;
            SteamUserName = bot.SteamUserName;
            SteamPassword = bot.SteamPassword;
        }

        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string SteamUserName { get; set; }
        
        [DataMember]
        public string SteamPassword { get; set; }
    }
}
