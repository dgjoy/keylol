using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// Steam 机器人延迟操作 DTO
    /// </summary>
    [DataContract]
    public class SteamBotDelayedActionDto
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        [DataMember]
        public SteamBotDelayedActionType Type { get; set; }

        /// <summary>
        /// 额外属性
        /// </summary>
        [DataMember]
        public dynamic Properties { get; set; }
    }

    [DataContract]
    public enum SteamBotDelayedActionType
    {
        [EnumMember]
        SendChatMessage,

        [EnumMember]
        RemoveFriend
    }
}
