using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     SteamLoginToken DTO
    /// </summary>
    [DataContract]
    public class SteamLoginTokenDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     登录代码
        /// </summary>
        [DataMember]
        public string Code { get; set; }
    }
}