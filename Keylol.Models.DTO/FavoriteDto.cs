using System;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     Favorite DTO
    /// </summary>
    [DataContract]
    public class FavoriteDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     据点类型
        /// </summary>
        [DataMember]
        public FavoritePointType Type { get; set; }

        /// <summary>
        ///     据点识别码
        /// </summary>
        [DataMember]
        public string IdCode { get; set; }

        /// <summary>
        ///     据点名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }

    /// <summary>
    ///     据点类型
    /// </summary>
    [DataContract]
    public enum FavoritePointType
    {
        /// <summary>
        ///     普通据点
        /// </summary>
        [EnumMember]
        NormalPoint,

        /// <summary>
        ///     个人据点
        /// </summary>
        [EnumMember]
        ProfilePoint
    }
}