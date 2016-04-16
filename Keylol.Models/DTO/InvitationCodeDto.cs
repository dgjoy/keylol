using System;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     InvitationCode DTO
    /// </summary>
    [DataContract]
    public class InvitationCodeDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     生成时间
        /// </summary>
        [DataMember]
        public DateTime? GenerateTime { get; set; }

        /// <summary>
        ///     来源标记
        /// </summary>
        [DataMember]
        public string Source { get; set; }
    }
}