using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     NormalPoint DTO
    /// </summary>
    [DataContract]
    public class NormalPointDto
    {
        /// <summary>
        ///     创建空 DTO，需要手动填充
        /// </summary>
        public NormalPointDto()
        {
        }

        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        /// <param name="point"><see cref="NormalPoint" /> 对象</param>
        /// <param name="nameOnly">是否仅包含名字</param>
        /// <param name="includeAliases">是否包含中英文索引</param>
        public NormalPointDto(NormalPoint point, bool nameOnly = false, bool includeAliases = false)
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

                if (includeAliases)
                {
                    EnglishAliases = point.EnglishAliases;
                    ChineseAliases = point.ChineseAliases;
                }
            }
        }

        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     头像
        /// </summary>
        [DataMember]
        public string AvatarImage { get; set; }

        /// <summary>
        ///     背景横幅
        /// </summary>
        [DataMember]
        public string BackgroundImage { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [DataMember]
        public NormalPointType? Type { get; set; }

        /// <summary>
        ///     识别码
        /// </summary>
        [DataMember]
        public string IdCode { get; set; }

        /// <summary>
        ///     中文名
        /// </summary>
        [DataMember]
        public string ChineseName { get; set; }

        /// <summary>
        ///     英文名
        /// </summary>
        [DataMember]
        public string EnglishName { get; set; }

        /// <summary>
        ///     英文索引
        /// </summary>
        [DataMember]
        public string EnglishAliases { get; set; }

        /// <summary>
        ///     中文索引
        /// </summary>
        [DataMember]
        public string ChineseAliases { get; set; }

        /// <summary>
        ///     主显名称偏好
        /// </summary>
        [DataMember]
        public PreferredNameType? PreferredName { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        ///     商店匹配名
        /// </summary>
        [DataMember]
        public string NameInSteamStore { get; set; }

        /// <summary>
        ///     读者数量
        /// </summary>
        [DataMember]
        public int? SubscriberCount { get; set; }

        /// <summary>
        ///     文章数量
        /// </summary>
        [DataMember]
        public int? ArticleCount { get; set; }

        /// <summary>
        ///     五分得票计数
        /// </summary>
        [DataMember]
        public Dictionary<int, int> VoteStats { get; set; }

        /// <summary>
        ///     是否被当前用户订阅
        /// </summary>
        [DataMember]
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 计数，仅用于搜索
        /// </summary>
        [DataMember]
        public int? Count { get; set; }

        #region Game Point Only

        /// <summary>
        ///     Steam App ID
        /// </summary>
        [DataMember]
        public int? SteamAppId { get; set; }

        /// <summary>
        ///     别名
        /// </summary>
        [DataMember]
        public string DisplayAliases { get; set; }

        /// <summary>
        ///     发行日期
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        ///     封面图片
        /// </summary>
        [DataMember]
        public string CoverImage { get; set; }

        /// <summary>
        ///     开发商据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> DeveloperPoints { get; set; }

        /// <summary>
        ///     发行商据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> PublisherPoints { get; set; }

        /// <summary>
        ///     流派据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> GenrePoints { get; set; }

        /// <summary>
        ///     特性据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> TagPoints { get; set; }

        /// <summary>
        ///     主要平台据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> MajorPlatformPoints { get; set; }

        /// <summary>
        ///     次要平台据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> MinorPlatformPoints { get; set; }

        /// <summary>
        ///     系列据点
        /// </summary>
        [DataMember]
        public List<NormalPointDto> SeriesPoints { get; set; }

        #endregion

        #region Platform / Manufacture Point Only

        /// <summary>
        ///     开发的游戏数量
        /// </summary>
        [DataMember]
        public int? GameCountAsDeveloper { get; set; }

        /// <summary>
        ///     发行的游戏数量
        /// </summary>
        [DataMember]
        public int? GameCountAsPublisher { get; set; }

        /// <summary>
        ///     此流派的游戏数量
        /// </summary>
        [DataMember]
        public int? GameCountAsGenre { get; set; }

        /// <summary>
        ///     此特性的游戏数量
        /// </summary>
        [DataMember]
        public int? GameCountAsTag { get; set; }

        /// <summary>
        ///     此系列的游戏数量
        /// </summary>
        [DataMember]
        public int? GameCountAsSeries { get; set; }

        #endregion
    }
}