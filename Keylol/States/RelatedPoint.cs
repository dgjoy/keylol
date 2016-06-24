using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.States.Shared;
using Keylol.StateTreeManager;

namespace Keylol.States
{
    /// <summary>
    /// 关联投稿据点列表
    /// </summary>
    public class RelatedPointList : List<PointBasicInfo>
    {
        private RelatedPointList([NotNull] IEnumerable<PointBasicInfo> collection) : base(collection)
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
                    relationship.TargetPoint.Type,
                    relationship.TargetPoint.AvatarImage,
                    relationship.TargetPoint.ChineseName,
                    relationship.TargetPoint.EnglishName
                }).Distinct().Take(10).ToListAsync())
                .Select(p => new PointBasicInfo
                {
                    Id = p.Id,
                    Type = p.Type,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName
                }));
        }
    }
}