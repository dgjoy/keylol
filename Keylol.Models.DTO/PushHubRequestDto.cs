using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// 内容推送请求 DTO
    /// </summary>
    [DataContract]
    public class PushHubRequestDto
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        [DataMember]
        public ContentPushType Type { get; set; }

        /// <summary>
        /// 内容 ID
        /// </summary>
        [DataMember]
        public string ContentId { get; set; }
    }

    /// <summary>
    /// 内容推送类型
    /// </summary>
    [DataContract]
    public enum ContentPushType
    {
        /// <summary>
        /// 文章
        /// </summary>
        [EnumMember] Article,

        /// <summary>
        /// 动态
        /// </summary>
        [EnumMember] Activity,

        /// <summary>
        /// 认可
        /// </summary>
        [EnumMember] Like
    }
}