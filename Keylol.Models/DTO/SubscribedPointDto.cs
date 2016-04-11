using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     <see cref="NormalPointDto" /> 与 <see cref="UserDto" /> 的融合，外加一些额外的共同属性
    /// </summary>
    [DataContract]
    public class SubscribedPointDto
    {
        /// <summary>
        ///     如果是普通据点，则有这个属性
        /// </summary>
        [DataMember]
        public NormalPointDto NormalPoint { get; set; }

        /// <summary>
        ///     如果是个人据点，则有这个属性
        /// </summary>
        [DataMember]
        public UserDto User { get; set; }

        /// <summary>
        ///     文章数量
        /// </summary>
        [DataMember]
        public int? ArticleCount { get; set; }

        /// <summary>
        ///     读者数量
        /// </summary>
        [DataMember]
        public int? SubscriberCount { get; set; }
    }
}