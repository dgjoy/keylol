using System;
using System.Collections.Generic;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// NormalPoint DTO
    /// </summary>
    public class NormalPointDto
    {
        public NormalPointDto()
        {
        }

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
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 背景横幅
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public NormalPointType? Type { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 英文索引
        /// </summary>
        public string EnglishAliases { get; set; }

        /// <summary>
        /// 中文索引
        /// </summary>
        public string ChineseAliases { get; set; }

        /// <summary>
        /// 主显名称偏好
        /// </summary>
        public PreferredNameType? PreferredName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 商店匹配名
        /// </summary>
        public string NameInSteamStore { get; set; }

        /// <summary>
        /// 读者数量
        /// </summary>
        public int? SubscriberCount { get; set; }

        /// <summary>
        /// 文章数量
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 五分得票计数
        /// </summary>
        public Dictionary<int, int> VoteStats { get; set; }

        /// <summary>
        /// 是否被当前用户订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        internal int? Count { get; set; }

        #region Game Point Only

        /// <summary>
        /// Steam App ID
        /// </summary>
        public int? SteamAppId { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string DisplayAliases { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// 封面图片
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 开发商据点
        /// </summary>
        public List<NormalPointDto> DeveloperPoints { get; set; }

        /// <summary>
        /// 发行商据点
        /// </summary>
        public List<NormalPointDto> PublisherPoints { get; set; }

        /// <summary>
        /// 流派据点
        /// </summary>
        public List<NormalPointDto> GenrePoints { get; set; }

        /// <summary>
        /// 特性据点
        /// </summary>
        public List<NormalPointDto> TagPoints { get; set; }

        /// <summary>
        /// 主要平台据点
        /// </summary>
        public List<NormalPointDto> MajorPlatformPoints { get; set; }

        /// <summary>
        /// 次要平台据点
        /// </summary>
        public List<NormalPointDto> MinorPlatformPoints { get; set; }

        /// <summary>
        /// 系列据点
        /// </summary>
        public List<NormalPointDto> SeriesPoints { get; set; }

        #endregion

        #region Platform / Manufacture Point Only

        /// <summary>
        /// 开发的游戏数量
        /// </summary>
        public int? GameCountAsDeveloper { get; set; }

        /// <summary>
        /// 发行的游戏数量
        /// </summary>
        public int? GameCountAsPublisher { get; set; }

        /// <summary>
        /// 此流派的游戏数量
        /// </summary>
        public int? GameCountAsGenre { get; set; }

        /// <summary>
        /// 此特性的游戏数量
        /// </summary>
        public int? GameCountAsTag { get; set; }

        /// <summary>
        /// 此系列的游戏数量
        /// </summary>
        public int? GameCountAsSeries { get; set; }

        #endregion
    }
}