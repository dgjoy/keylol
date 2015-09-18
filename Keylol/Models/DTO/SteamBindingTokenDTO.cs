using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class SteamBindingTokenDTO
    {
        public SteamBindingTokenDTO(SteamBindingToken token)
        {
            Id = token.Id;
            Code = token.Code;
            BotId = token.Bot.Id;
        }

        public string Id { get; set; }
        public string Code { get; set; }
        public string BotId { get; set; }
    }
}