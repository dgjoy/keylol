using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;

namespace Keylol.Provider.CachedDataProvider
{
    /// <summary>
    /// 负责据点相关操作
    /// </summary>
    public class PointOperations
    {
        private readonly KeylolDbContext _dbContext;
        private readonly RedisProvider _redis;
        private static readonly TimeSpan RatingUpdatePeriod = TimeSpan.FromHours(12);

        /// <summary>
        /// 创建 <see cref="PointOperations"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public PointOperations(KeylolDbContext dbContext, RedisProvider redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        private static string RatingCacheKey(string pointId) => $"point-rating:{pointId}";

        /// <summary>
        /// 获取指定据点的评分
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <exception cref="ArgumentNullException"><paramref name="pointId"/> 为 null</exception>
        /// <returns>据点评分</returns>
        public async Task<PointRatingsDto> GetRatingsAsync([NotNull] string pointId)
        {
            if (pointId == null)
                throw new ArgumentNullException(nameof(pointId));

            var cacheKey = RatingCacheKey(pointId);
            var redisDb = _redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return RedisProvider.Deserialize<PointRatingsDto>(cacheResult);

            var ratings = new PointRatingsDto();
            var userRatings = new Dictionary<string, UserRating>();
            foreach (var ratingEntry in await _dbContext.Articles
                .Where(a => a.TargetPointId == pointId && a.Rating != null &&
                            a.Archived == ArchivedState.None && a.Rejected == false)
                .Select(a => new {a.AuthorId, a.Rating})
                .Union(_dbContext.Activities
                    .Where(a => a.TargetPointId == pointId && a.Rating != null)
                    .Select(a => new {a.AuthorId, a.Rating}))
                .ToListAsync())
            {
                switch (ratingEntry.Rating.Value)
                {
                    case 1:
                        ratings.OneStarCount++;
                        break;

                    case 2:
                        ratings.TwoStarCount++;
                        break;

                    case 3:
                        ratings.ThreeStarCount++;
                        break;

                    case 4:
                        ratings.FourStarCount++;
                        break;

                    case 5:
                        ratings.FiveStarCount++;
                        break;

                    default:
                        continue;
                }
                UserRating userRating;
                if (!userRatings.TryGetValue(ratingEntry.AuthorId, out userRating))
                    userRatings[ratingEntry.AuthorId] = userRating = new UserRating();
                userRating.Count++;
                userRating.Total += ratingEntry.Rating.Value;
            }
            ratings.AverageRating = userRatings.Count < 3
                ? (double?) null
                : Math.Round(userRatings.Values.Sum(r => r.Total*2/(double) r.Count)/userRatings.Count, 1);
            await redisDb.StringSetAsync(cacheKey, RedisProvider.Serialize(ratings), RatingUpdatePeriod);
            return ratings;
        }

        private class UserRating
        {
            public int Total { get; set; }

            public int Count { get; set; }
        }
    }
}