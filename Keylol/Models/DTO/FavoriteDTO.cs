using System;

namespace Keylol.Models.DTO
{
    public class FavoriteDTO
    {
        public enum PointType
        {
            Unknown,
            NormalPoint,
            ProfilePoint
        }

        public FavoriteDTO(Favorite favorite)
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
                var normalPoint = favorite.Point as NormalPoint;
                if (normalPoint != null)
                {
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
                else
                {
                    Type = PointType.Unknown;
                }
            }
        }

        public string Id { get; set; }
        public PointType Type { get; set; }
        public string IdCode { get; set; }
        public string Name { get; set; }
    }
}