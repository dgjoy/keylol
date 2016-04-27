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
        ///     据点类型
        /// </summary>
        [DataContract]
        public enum PointType
        {
            /// <summary>
            ///     普通据点
            /// </summary>
            [EnumMember] NormalPoint,

            /// <summary>
            ///     个人据点
            /// </summary>
            [EnumMember] ProfilePoint
        }

        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        /// <param name="favorite"><see cref="Favorite" /> 对象</param>
        public FavoriteDto(Favorite favorite)
        {
            Id = favorite.Id;
            var profilePoint = favorite.Point as ProfilePoint;
            if (profilePoint != null)
            {
                Type = PointType.ProfilePoint;
                IdCode = profilePoint.User.IdCode;
                Name = profilePoint.User.UserName;
            }
            else
            {
                var normalPoint = (NormalPoint) favorite.Point;
                Type = PointType.NormalPoint;
                IdCode = normalPoint.IdCode;
                switch (normalPoint.PreferredName)
                {
                    case PreferredNameType.Chinese:
                        Name = normalPoint.ChineseName;
                        break;

                    case PreferredNameType.English:
                        Name = normalPoint.EnglishName;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     据点类型
        /// </summary>
        [DataMember]
        public PointType Type { get; set; }

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
}