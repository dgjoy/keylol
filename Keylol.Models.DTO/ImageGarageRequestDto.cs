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
        /// 内容类型
        /// </summary>
        [DataMember]
        public ImageGarageRequestContentType ContentType { get; set; }

        /// <summary>
        ///     内容 ID
        /// </summary>
        [DataMember]
        public string ContentId { get; set; }
    }

    /// <summary>
    /// Image Garage 请求内容类型
    /// </summary>
    [DataContract]
    public enum ImageGarageRequestContentType
    {
        /// <summary>
        /// 文章
        /// </summary>
        [EnumMember] Article,

        /// <summary>
        /// 文章评论
        /// </summary>
        [EnumMember] ArticleComment
    }
}