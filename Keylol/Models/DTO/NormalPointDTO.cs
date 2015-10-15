using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class NormalPointDTO
    {
        public NormalPointDTO(NormalPoint point)
        {
            Id = point.Id;
            AvatarImage = point.AvatarImage;
            BackgroundImage = point.BackgroundImage;
            Type = point.Type;
            IdCode = point.IdCode;
            ChineseName = point.ChineseName;
            EnglishName = point.EnglishName;
            StoreLink = point.StoreLink;
        }

        public string Id { get; set; }
        public string AvatarImage { get; set; }
        public string BackgroundImage { get; set; }
        public NormalPointType Type { get; set; }
        public string IdCode { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }
        public string StoreLink { get; set; }
    }
}
