using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Provider;
using Keylol.Utilities;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        private readonly StatisticsProvider _statistics;
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建 <see cref="DatabaseMigrationController"/>
        /// </summary>
        /// <param name="statistics"><see cref="StatisticsProvider"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public DatabaseMigrationController(StatisticsProvider statistics, CouponProvider coupon)
        {
            _statistics = statistics;
            _coupon = coupon;
        }

        /// <summary>
        /// 为老用户导入初测期间获得的认可对应的文券奖励
        /// </summary>
        [Route("coupon-for-old-user")]
        [HttpPost]
        public async Task<IHttpActionResult> CouponForOldUser()
        {
            foreach (var user in await DbContext.Users
                .Where(u => u.Coupon == 0).OrderBy(u => u.SequenceNumber).ToListAsync())
            {
                var likeCount = await _statistics.GetUserLikeCount(user.Id);
                await _coupon.Update(user.Id, CouponEvent.新注册, logTime: user.RegisterTime);
                if (likeCount > 0)
                    await _coupon.Update(user.Id, likeCount, "导入初测期间已获得的认可");
            }
            return Ok("迁移成功");
        }
    }
}