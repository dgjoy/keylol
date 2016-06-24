using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// Reliable Notification DTO
    /// </summary>
    [DataContract]
    public class ReliableNotificationDto
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        [DataMember]
        public string MethodName { get; set; }

        /// <summary>
        /// 方法参数
        /// </summary>
        [DataMember]
        public List<ReliableNotificationArgumentDto> Arguments { get; set; }
    }

    /// <summary>
    /// Reliable Notification Argument DTO
    /// </summary>
    [DataContract]
    public class ReliableNotificationArgumentDto
    {
        /// <summary>
        /// 类型
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [DataMember]
        public object Value { get; set; }
    }
}