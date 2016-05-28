using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Entrance.Points
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
        /// 获取可能感兴趣的据点列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="InterestedPointList"/></returns>
        public static async Task<InterestedPointList> Get(int page, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="InterestedPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="InterestedPointList"/></returns>
        public static async Task<InterestedPointList> CreateAsync(string currentUserId, int page,
            KeylolDbContext dbContext)
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
                    group 1 by relationship.TargetPoint
                    into g
                  where !dbContext.Subscriptions.Any(s => s.SubscriberId == currentUserId &&
                                                            s.TargetId == g.Key.Id &&
                                                            s.TargetType == SubscriptionTargetType.Point)
                    orderby g.Count() descending
                    select new
                    {
                        g.Key.Id,
                        g.Key.IdCode,
                        g.Key.AvatarImage,
                        g.Key.ChineseName,
                        g.Key.EnglishName,
                        GameCount = dbContext.PointRelationships.Where(r => r.TargetPointId == g.Key.Id)
                            .Select(r => r.SourcePointId)
                            .Distinct()
                            .Count()
                    }).TakePage(page, 9).ToListAsync())
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