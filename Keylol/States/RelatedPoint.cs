using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States
{
    /// <summary>
    /// 关联投稿据点列表
    /// </summary>
    public class RelatedPointList : List<RelatedPoint>
    {
        private RelatedPointList([NotNull] IEnumerable<RelatedPoint> collection) : base(collection)
        {
        }

        /// <summary>
        /// 获取指定据点的关联投稿据点列表
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="RelatedPointList"/></returns>
        public static async Task<RelatedPointList> Get(string pointId, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(pointId, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="RelatedPointList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="RelatedPointList"/></returns>
        public static async Task<RelatedPointList> CreateAsync(string pointId, KeylolDbContext dbContext)
        {
            return new RelatedPointList((await (from relationship in dbContext.PointRelationships
                where relationship.SourcePointId == pointId &&
                      (relationship.Relationship == PointRelationshipType.Developer ||
                       relationship.Relationship == PointRelationshipType.Manufacturer ||
                       relationship.Relationship == PointRelationshipType.Series ||
                       relationship.Relationship == PointRelationshipType.Tag)
                select new
                {
                    relationship.TargetPoint.Id,
                    relationship.TargetPoint.AvatarImage,
                    relationship.TargetPoint.ChineseName,
                    relationship.TargetPoint.EnglishName
                }).Distinct().Take(10).ToListAsync())
                .Select(p => new RelatedPoint
                {
                    Id = p.Id,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName
                }));
        }
    }

    /// <summary>
    /// 关联投稿据点
    /// </summary>
    public class RelatedPoint
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

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
    }
}