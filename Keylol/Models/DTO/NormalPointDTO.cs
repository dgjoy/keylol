using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class NormalPointDTO
    {
        public NormalPointDTO(NormalPoint point, bool nameOnly = false)
        {
            Id = point.Id;
            PreferedName = point.PreferedName;
            if (nameOnly)
            {
                switch (point.PreferedName)
                {
                    case PreferedNameType.Chinese:
                        ChineseName = point.ChineseName;
                        break;
                    case PreferedNameType.English:
                        EnglishName = point.EnglishName;
                        break;
                }
            }
            else
            {
                ChineseName = point.ChineseName;
                EnglishName = point.EnglishName;
                AvatarImage = point.AvatarImage;
                BackgroundImage = point.BackgroundImage;
                Type = point.Type;
                IdCode = point.IdCode;
                StoreLink = point.StoreLink;
            }
        }

        public string Id { get; set; }
        public string AvatarImage { get; set; }
        public string BackgroundImage { get; set; }
        public NormalPointType Type { get; set; }
        public string IdCode { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }
        public PreferedNameType PreferedName { get; set; }
        public string StoreLink { get; set; }
    }
}