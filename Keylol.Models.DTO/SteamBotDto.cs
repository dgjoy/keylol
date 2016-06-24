using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     SteamBot DTO
    /// </summary>
    [DataContract]
    public class SteamBotDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     序号
        /// </summary>
        [DataMember]
        public int? SequenceNumber { get; set; }

        /// <summary>
        ///     Steam 用户名
        /// </summary>
        [DataMember]
        public string SteamUserName { get; set; }

        /// <summary>
        ///     Steam 密码
        /// </summary>
        [DataMember]
        public string SteamPassword { get; set; }

        /// <summary>
        ///     Steam ID 3
        /// </summary>
        [DataMember]
        public string SteamId { get; set; }

        /// <summary>
        ///     Steam ID 64
        /// </summary>
        [DataMember]
        public string SteamId64 { get; set; }

        /// <summary>
        ///     是否在线
        /// </summary>
        [DataMember]
        public bool Online { get; set; }
    }
}