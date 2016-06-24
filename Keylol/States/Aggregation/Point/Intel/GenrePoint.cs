using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.Point.Intel
{
    /// <summary>
    /// 流派据点列表
    /// </summary>
    public class GenrePointList : List<GenrePoint>
    {
        private GenrePointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="GenrePointList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="GenrePointList"/></returns>
        public static async Task<GenrePointList> CreateAsync(string pointId, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from relationship in dbContext.PointRelationships
                where relationship.SourcePointId == pointId && relationship.Relationship == PointRelationshipType.Genre
                select new
                {
                    relationship.TargetPoint.Id,
                    relationship.TargetPoint.IdCode,
                    relationship.TargetPoint.AvatarImage,
                    relationship.TargetPoint.ChineseName,
                    relationship.TargetPoint.EnglishName,
                    GameCount = dbContext.PointRelationships
                        .Where(r => r.TargetPointId == relationship.TargetPointId)
                        .GroupBy(r => r.SourcePointId)
                        .Count()
                }).ToListAsync();

            var result = new GenrePointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new GenrePoint
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    GameCount = p.GameCount,
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                            SubscriptionTargetType.Point)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 流派据点
    /// </summary>
    public class GenrePoint
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 游戏数
        /// </summary>
        public int GameCount { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}