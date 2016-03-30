using System.Runtime.Serialization;
using SteamKit2;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// SteamBot DTO
    /// </summary>
    [DataContract]
    public class SteamBotDto
    {
        public SteamBotDto(SteamBot bot, bool includeCredentials = false)
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

        /// <summary>
        /// Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Steam 用户名
        /// </summary>
        [DataMember]
        public string SteamUserName { get; set; }

        /// <summary>
        /// Steam 密码
        /// </summary>
        [DataMember]
        public string SteamPassword { get; set; }

        /// <summary>
        /// Steam ID 3
        /// </summary>
        [DataMember]
        public string SteamId { get; set; }

        /// <summary>
        /// Steam ID 64
        /// </summary>
        [DataMember]
        public string SteamId64 { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        [DataMember]
        public bool Online { get; set; }
    }
}