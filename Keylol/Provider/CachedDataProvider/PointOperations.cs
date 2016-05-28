using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
            {
                await redisDb.KeyExpireAsync(cacheKey, CachedDataProvider.DefaultTtl);
                return RedisProvider.Deserialize<PointRatingsDto>(cacheResult);
            }

            // TODO
            var random = new Random();
            var rating = new PointRatingsDto
            {
                OneStarCount = random.Next(0, 100),
                TwoStarCount = random.Next(0, 100),
                ThreeStarCount = random.Next(0, 100),
                FourStarCount = random.Next(0, 100),
                FiveStarCount = random.Next(0, 100),
                TotalScore = random.Next(200, 1001),
                TotalCount = 100
            };
//                await redisDb.StringSetAsync(cacheKey, RedisProvider.Serialize(rating), DefaultTtl);
            return rating;
        }
    }
}