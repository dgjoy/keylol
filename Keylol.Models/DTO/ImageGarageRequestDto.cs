using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     Image Garage 请求 DTO
    /// </summary>
    [DataContract]
    public class ImageGarageRequestDto
    {
        /// <summary>
        ///     文章 ID
        /// </summary>
        [DataMember]
        public string ArticleId { get; set; }
    }
}