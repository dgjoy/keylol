using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;

namespace Keylol.States.Entrance.PointsPage
{
    /// <summary>
    /// 可能感兴趣的据点列表
    /// </summary>
    public class InterestedPointList : List<InterestedPoint>
    {
        private InterestedPointList([NotNull] IEnumerable<InterestedPoint> collection) : base(collection)
        {
        }

        /// <summary>
        /// 创建 <see cref="InterestedPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="InterestedPointList"/></returns>
        public static async Task<InterestedPointList> CreateAsync(string currentUserId, KeylolDbContext dbContext)
        {
            return new InterestedPointList((await (string.IsNullOrWhiteSpace(currentUserId)
                ? from point in dbContext.Points
                    where point.Type == PointType.Category
                    let gameCount = dbContext.PointRelationships.Where(r => r.TargetPointId == point.Id)
                        .Select(r => r.SourcePointId)
                        .Distinct()
                        .Count()
                    orderby gameCount descending
                    select new
                    {
                        point.Id,
                        point.IdCode,
                        point.AvatarImage,
                        point.ChineseName,
                        point.EnglishName,
                        GameCount = gameCount
                    }
                : from subscription in dbContext.Subscriptions
                    where subscription.SubscriberId == currentUserId &&
                          subscription.TargetType == SubscriptionTargetType.Point
                    join relationship in dbContext.PointRelationships
                        on subscription.TargetId equals relationship.SourcePointId
                    where relationship.Relationship == PointRelationshipType.Tag ||
                          relationship.Relationship == PointRelationshipType.Series
                    group relationship by relationship.TargetPointId
                    into g
                    where !dbContext.Subscriptions.Any(s => s.SubscriberId == currentUserId &&
                                                            s.TargetId == g.Key &&
                                                            s.TargetType == SubscriptionTargetType.Point)
                    join point in dbContext.Points on g.Key equals point.Id
                    orderby g.Count() descending
                    select new
                    {
                        point.Id,
                        point.IdCode,
                        point.AvatarImage,
                        point.ChineseName,
                        point.EnglishName,
                        GameCount = dbContext.PointRelationships.Where(r => r.TargetPointId == g.Key)
                            .Select(r => r.SourcePointId)
                            .Distinct()
                            .Count()
                    }).Take(9).ToListAsync())
                .Select(p => new InterestedPoint
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    GameCount = p.GameCount
                }));
        }
    }

    /// <summary>
    /// 可能感兴趣的据点
    /// </summary>
    public class InterestedPoint
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
        /// 旗下游戏数量
        /// </summary>
        public int GameCount { get; set; }
    }
}