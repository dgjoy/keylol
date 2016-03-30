using System;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// InvitationCode DTO
    /// </summary>
    public class InvitationCodeDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime? GenerateTime { get; set; }

        /// <summary>
        /// 来源标记
        /// </summary>
        public string Source { get; set; }
    }
}