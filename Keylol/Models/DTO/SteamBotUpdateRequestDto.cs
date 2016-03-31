using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     SteamBot 更新请求 DTO
    /// </summary>
    [DataContract]
    public class SteamBotUpdateRequestDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        [Required]
        public string Id { get; set; }

        /// <summary>
        ///     Steam ID 3
        /// </summary>
        [DataMember]
        public string SteamId { get; set; }

        /// <summary>
        ///     好友数量
        /// </summary>
        [DataMember]
        public int? FriendCount { get; set; }

        /// <summary>
        ///     是否在线
        /// </summary>
        [DataMember]
        public bool? Online { get; set; }
    }
}