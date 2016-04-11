using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    [DataContract]
    public class ImageGarageRequestDto
    {
        [DataMember]
        public string ArticleId { get; set; }
    }
}