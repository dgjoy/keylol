using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DAL;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize(Users = "Stackia")]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="DatabaseMigrationController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public DatabaseMigrationController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 添加一个据点职员
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="staffId">职员 ID</param>
        [Route("add-point-staff")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPointStaff(string pointId, string staffId)
        {
            _dbContext.PointStaff.Add(new PointStaff
            {
                PointId = pointId,
                StaffId = staffId
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 重新计算所有用户的当季文章认可数 （2016年，6月1日 - 8月31日）
        /// </summary>
        [Route("recalculate-season-article-like")]
        [HttpPost]
        public async Task<IHttpActionResult> RecalculateSeasonArticleLike()
        {
            var users = await _dbContext.Users.ToListAsync();
            var startTime = new DateTime(2016, 6, 1, 0, 0, 0);
            var endTime = new DateTime(2016, 9, 1, 0, 0, 0);
            foreach (var user in users)
            {
                user.SeasonLikeCount = await (from article in _dbContext.Articles
                    join like in _dbContext.Likes on article.Id equals like.TargetId
                    where like.TargetType == LikeTargetType.Article && article.AuthorId == user.Id &&
                          like.Time >= startTime && like.Time < endTime
                    select article
                    ).CountAsync();
                await _dbContext.SaveChangesAsync();
            }
            return Ok();
        }

        /// <summary>
        /// 同步数据库原先所有文券商品交易时价格 (2016年8月4日前)
        /// </summary>
        [Route("sync-coupon-gift-order-redeem-price")]
        [HttpPost]
        public async Task<IHttpActionResult> SyncCouponGiftOrderRedeemPrice()
        {
            var couponGiftOrders = await _dbContext.CouponGiftOrders
                .Include(o=>o.Gift)
                .ToListAsync();
            foreach (var couponGiftOrder in couponGiftOrders)
            {
                couponGiftOrder.RedeemPrice = couponGiftOrder.Gift.Price;
                await _dbContext.SaveChangesAsync();
            }
            return Ok();
        }
    }
}