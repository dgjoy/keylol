using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;
using Keylol.States.Aggregation.Point.BasicInfo;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point.Frontpage
{
    /// <summary>
    /// 聚合 - 据点 - 扉页
    /// </summary>
    public class FrontpagePage
    {
        /// <summary>
        /// 获取据点扉页
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="FrontpagePage"/></returns>
        public static async Task<FrontpagePage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new FrontpagePage();
            return await CreateAsync(point, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="FrontpagePage"/>
        /// </summary>
        /// <param name="point">已经查询好的据点对象</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="FrontpagePage"/></returns>
        public static async Task<FrontpagePage> CreateAsync(Models.Point point, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var frontPage = new FrontpagePage();

            if (point.Type == PointType.Game || point.Type == PointType.Hardware)
            {
                frontPage.MediaHeaderImage = point.MediaHeaderImage;
                frontPage.Media = Helpers.SafeDeserialize<List<PointMedia>>(point.Media);

                frontPage.SimilarPoints =
                    await SimilarPointList.CreateAsync(point.Id, currentUserId, 1, dbContext, cachedData);
            }

            if (point.Type == PointType.Game)
            {
                frontPage.Platforms = (await (from relationship in dbContext.PointRelationships
                    where relationship.SourcePointId == point.Id &&
                          relationship.Relationship == PointRelationshipType.Platform
                    select new
                    {
                        relationship.TargetPoint.IdCode,
                        relationship.TargetPoint.ChineseName,
                        relationship.TargetPoint.EnglishName
                    })
                    .ToListAsync())
                    .Select(p => new SimplePoint
                    {
                        IdCode = p.IdCode,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName
                    })
                    .ToList();

                #region 特性属性

                frontPage.MultiPlayer = point.MultiPlayer ? true : (bool?) null;
                frontPage.SinglePlayer = point.SinglePlayer ? true : (bool?) null;
                frontPage.Coop = point.Coop ? true : (bool?) null;
                frontPage.CaptionsAvailable = point.CaptionsAvailable ? true : (bool?) null;
                frontPage.CommentaryAvailable = point.CommentaryAvailable ? true : (bool?) null;
                frontPage.IncludeLevelEditor = point.IncludeLevelEditor ? true : (bool?) null;
                frontPage.Achievements = point.Achievements ? true : (bool?) null;
                frontPage.Cloud = point.Cloud ? true : (bool?) null;
                frontPage.LocalCoop = point.LocalCoop ? true : (bool?) null;
                frontPage.SteamTradingCards = point.SteamTradingCards ? true : (bool?) null;
                frontPage.SteamWorkshop = point.SteamWorkshop ? true : (bool?) null;
                frontPage.InAppPurchases = point.InAppPurchases ? true : (bool?) null;

                #endregion

                frontPage.ChineseAvailability = Helpers.SafeDeserialize<ChineseAvailability>(point.ChineseAvailability);

                frontPage.AddictedUsers =
                    await AddictedUserList.CreateAsync(currentUserId, point.SteamAppId, 1, dbContext, cachedData);
            }

            return frontPage;
        }

        /// <summary>
        /// 平台
        /// </summary>
        public List<SimplePoint> Platforms { get; set; }

        #region 特性属性

        /// <summary>
        /// 多人游戏
        /// </summary>
        public bool? MultiPlayer { get; set; }

        /// <summary>
        /// 单人游戏
        /// </summary>
        public bool? SinglePlayer { get; set; }

        /// <summary>
        /// 合作
        /// </summary>
        public bool? Coop { get; set; }

        /// <summary>
        /// 视听字幕
        /// </summary>
        public bool? CaptionsAvailable { get; set; }

        /// <summary>
        /// 旁白解说
        /// </summary>
        public bool? CommentaryAvailable { get; set; }

        /// <summary>
        /// 关卡客制化
        /// </summary>
        public bool? IncludeLevelEditor { get; set; }

        /// <summary>
        /// 成就系统
        /// </summary>
        public bool? Achievements { get; set; }

        /// <summary>
        /// 云存档
        /// </summary>
        public bool? Cloud { get; set; }

        /// <summary>
        /// 本地多人
        /// </summary>
        public bool? LocalCoop { get; set; }

        /// <summary>
        /// Steam 卡牌
        /// </summary>
        public bool? SteamTradingCards { get; set; }

        /// <summary>
        /// Steam 创意工坊
        /// </summary>
        public bool? SteamWorkshop { get; set; }

        /// <summary>
        /// 内购
        /// </summary>
        public bool? InAppPurchases { get; set; }

        #endregion

        /// <summary>
        /// 华语可用度
        /// </summary>
        public ChineseAvailability ChineseAvailability { get; set; }

        /// <summary>
        /// 媒体中心头部图
        /// </summary>
        public string MediaHeaderImage { get; set; }

        /// <summary>
        /// 媒体中心
        /// </summary>
        public List<PointMedia> Media { get; set; }

        /// <summary>
        /// 入坑用户
        /// </summary>
        public AddictedUserList AddictedUsers { get; set; }

        /// <summary>
        /// 近畿据点
        /// </summary>
        public SimilarPointList SimilarPoints { get; set; }
    }
}