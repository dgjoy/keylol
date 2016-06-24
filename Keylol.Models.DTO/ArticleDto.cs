using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     Article DTO
    /// </summary>
    [DataContract]
    public class ArticleDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     标题
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        ///     RowVersion
        /// </summary>
        [DataMember]
        public byte[] RowVersion { get; set; }

        /// <summary>
        ///     缩略图
        /// </summary>
        [DataMember]
        public string CoverImage { get; set; }
    }
}