using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace Keylol.Models.DTO
{
    [DataContract]
    public class SteamBotDTO
    {
        public SteamBotDTO(SteamBot bot, bool includeCredentials = false)
        {
            Id = bot.Id;

            if (includeCredentials)
            {
                SteamUserName = bot.SteamUserName;
                SteamPassword = bot.SteamPassword;
            }

            SteamId = bot.SteamId;
            var steamId = new SteamID();
            steamId.SetFromSteam3String(SteamId);
            SteamId64 = steamId.ConvertToUInt64().ToString();
            Online = bot.SessionId != null && bot.Online;
        }

        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string SteamUserName { get; set; }
        
        [DataMember]
        public string SteamPassword { get; set; }
        
        [DataMember]
        public string SteamId { get; set; }

        [DataMember]
        public string SteamId64 { get; set; }

        [DataMember]
        public bool Online { get; set; }
    }
}
