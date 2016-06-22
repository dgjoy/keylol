using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// ArticleComment DTO
    /// </summary>
    [DataContract]
    public class ArticleCommentDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        
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
    }
}
