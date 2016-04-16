using System;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     LoginLog DTO
    /// </summary>
    [DataContract]
    public class LoginLogDto
    {
        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        /// <param name="log"><see cref="LoginLog" /> 对象</param>
        public LoginLogDto(LoginLog log)
        {
            Id = log.Id;
            Time = log.Time;
            Ip = log.Ip;
            UserId = log.UserId;
        }

        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     时间
        /// </summary>
        [DataMember]
        public DateTime Time { get; set; }

        /// <summary>
        ///     所用 IP
        /// </summary>
        [DataMember]
        public string Ip { get; set; }

        /// <summary>
        ///     用户 ID
        /// </summary>
        [DataMember]
        public string UserId { get; set; }
    }
}