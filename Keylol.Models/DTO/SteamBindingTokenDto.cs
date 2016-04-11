using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     SteamBindingToken DTO
    /// </summary>
    [DataContract]
    public class SteamBindingTokenDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     绑定代码
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        ///     机器人 Steam ID 64
        /// </summary>
        [DataMember]
        public string BotSteamId64 { get; set; }
    }
}