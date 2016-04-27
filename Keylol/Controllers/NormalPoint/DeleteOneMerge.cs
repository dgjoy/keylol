using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Utilities;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        private void MergeCollection<T>(ICollection<T> targetCollection, IEnumerable<T> sourceCollection)
        {
            foreach (var item in sourceCollection.Where(item => !targetCollection.Contains(item)))
            {
                targetCollection.Add(item);
            }
        }

        /// <summary>
        ///     把一个据点合并至另一个据点
        /// </summary>
        /// <param name="sourceIdCode">原据点识别码，在据点合并之后将被删除</param>
        /// <param name="targetIdCode">合并到的目标据点识别码</param>
        [Route("{sourceIdCode}/merge/{targetIdCode}")]
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteOneMerge(string sourceIdCode, string targetIdCode)
        {
            var sourcePoint = await _dbContext.NormalPoints.Where(p => p.IdCode == sourceIdCode).SingleOrDefaultAsync();
            if (sourcePoint == null)
                return NotFound();

            var targetPoint = await _dbContext.NormalPoints.Where(p => p.IdCode == targetIdCode).SingleOrDefaultAsync();
            if (targetPoint == null)
                return NotFound();

            // 商店匹配名
            MergeCollection(targetPoint.SteamStoreNames, sourcePoint.SteamStoreNames);

            // 关联关系
            MergeCollection(targetPoint.DeveloperForPoints, sourcePoint.DeveloperForPoints);
            MergeCollection(targetPoint.PublisherForPoints, sourcePoint.PublisherForPoints);
            MergeCollection(targetPoint.SeriesForPoints, sourcePoint.SeriesForPoints);
            MergeCollection(targetPoint.TagForPoints, sourcePoint.TagForPoints);
            MergeCollection(targetPoint.GenreForPoints, sourcePoint.GenreForPoints);
            MergeCollection(targetPoint.MajorPlatformForPoints, sourcePoint.MajorPlatformForPoints);
            MergeCollection(targetPoint.MinorPlatformForPoints, sourcePoint.MinorPlatformForPoints);

            // 文章推送关系
            MergeCollection(targetPoint.Articles, sourcePoint.Articles);

            // 文章评价对象
            var articles = await _dbContext.Articles.Where(a => a.VoteForPointId == sourcePoint.Id).ToListAsync();
            foreach (var article in articles)
            {
                article.VoteForPointId = targetPoint.Id;
            }

            // 手动订阅关系
            MergeCollection(targetPoint.Subscribers, sourcePoint.Subscribers);

            // 自动订阅关系
            var sourceAutoSubscriptions =
                await _dbContext.AutoSubscriptions.Where(s => s.NormalPointId == sourcePoint.Id).ToListAsync();
            var targetAutoSubscriptions =
                await _dbContext.AutoSubscriptions.Where(s => s.NormalPointId == targetPoint.Id).ToListAsync();
            foreach (var sourceAutoSubscription in sourceAutoSubscriptions.Where(ss =>
                targetAutoSubscriptions.All(ts => ss.UserId != ts.UserId)))
            {
                var newSubscription = _dbContext.AutoSubscriptions.Create();
                newSubscription.UserId = sourceAutoSubscription.UserId;
                newSubscription.NormalPointId = targetPoint.Id;
                newSubscription.Type = sourceAutoSubscription.Type;
                newSubscription.DisplayOrder = sourceAutoSubscription.DisplayOrder;
                _dbContext.AutoSubscriptions.Add(newSubscription);
            }

            sourcePoint.SteamStoreNames.Clear();

            sourcePoint.DeveloperPoints.Clear();
            sourcePoint.PublisherPoints.Clear();
            sourcePoint.SeriesPoints.Clear();
            sourcePoint.TagPoints.Clear();
            sourcePoint.GenrePoints.Clear();
            sourcePoint.MajorPlatformPoints.Clear();
            sourcePoint.MinorPlatformPoints.Clear();

            sourcePoint.DeveloperForPoints.Clear();
            sourcePoint.PublisherForPoints.Clear();
            sourcePoint.SeriesForPoints.Clear();
            sourcePoint.TagForPoints.Clear();
            sourcePoint.GenreForPoints.Clear();
            sourcePoint.MajorPlatformForPoints.Clear();
            sourcePoint.MinorPlatformForPoints.Clear();

            sourcePoint.Articles.Clear();

            sourcePoint.Subscribers.Clear();
            _dbContext.AutoSubscriptions.RemoveRange(sourceAutoSubscriptions);

            _dbContext.NormalPoints.Remove(sourcePoint);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}