using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class NormalPointDTO
    {
        public NormalPointDTO()
        {
        }

        public NormalPointDTO(NormalPoint point, bool nameOnly = false, bool includeAliases = false)
        {
            Id = point.Id;
            PreferredName = point.PreferredName;
            IdCode = point.IdCode;
            if (nameOnly)
            {
                switch (point.PreferredName)
                {
                    case PreferredNameType.Chinese:
                        ChineseName = point.ChineseName;
                        break;
                    case PreferredNameType.English:
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

                if (point.Type == NormalPointType.Game)
                {
                    StoreLink = point.StoreLink;
                    SteamAppId = point.SteamAppId;
                    DisplayAliases = point.DisplayAliases;
                    ReleaseDate = point.ReleaseDate;
                    CoverImage = point.CoverImage;
                }


                if (includeAliases)
                {
                    EnglishAliases = point.EnglishAliases;
                    ChineseAliases = point.ChineseAliases;
                }
            }
        }

        public string Id { get; set; }
        public string AvatarImage { get; set; }
        public string BackgroundImage { get; set; }
        public NormalPointType Type { get; set; }
        public string IdCode { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }
        public string EnglishAliases { get; set; }
        public string ChineseAliases { get; set; }
        public PreferredNameType PreferredName { get; set; }

        #region Game Point Only

        public int? SteamAppId { get; set; }
        public string DisplayAliases { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string CoverImage { get; set; }
        public List<NormalPointDTO> DeveloperPoints { get; set; }
        public List<NormalPointDTO> PublisherPoints { get; set; }
        public List<NormalPointDTO> GenrePoints { get; set; }
        public List<NormalPointDTO> TagPoints { get; set; }
        public List<NormalPointDTO> MajorPlatformPoints { get; set; }
        public List<NormalPointDTO> MinorPlatformPoints { get; set; }

        #endregion

        public int? SubscriberCount { get; set; }
        public int? ArticleCount { get; set; }
        public Dictionary<int, int> VoteStats { get; set; }
        public bool? Subscribed { get; set; }

        #region Obselete

        public List<NormalPointDTO> AssociatedPoints { get; set; }

        public string StoreLink { get; set; }

        #endregion

        internal int? Count { get; set; }
    }
}