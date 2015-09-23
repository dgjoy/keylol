using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace Keylol.Models.DTO
{
    public class SteamBindingTokenDTO
    {
        public SteamBindingTokenDTO(SteamBindingToken token)
        {
            Id = token.Id;
            Code = token.Code;
            var steamId = new SteamID();
            steamId.SetFromSteam3String(token.Bot.SteamId);
            BotSteamId64 = steamId.ConvertToUInt64().ToString();
        }

        public string Id { get; set; }
        public string Code { get; set; }
        public string BotSteamId64 { get; set; }
    }
}