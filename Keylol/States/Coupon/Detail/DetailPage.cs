using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.Coupon.Detail
{
    /// <summary>
    /// 文券中心 - 明细
    /// </summary>
    public class DetailPage
    {
        /// <summary>
        /// 获取文券明细页
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="DetailPage"/></returns>
        public static async Task<DetailPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] KeylolUserManager userManager)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, userManager);
        }

        /// <summary>
        /// 创建 <see cref="DetailPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="DetailPage"/></returns>
        public static async Task<DetailPage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            KeylolUserManager userManager)
        {
            var couponLogs = await CouponLogList.CreateAsync(currentUserId, 1, true, dbContext, userManager);
            return new DetailPage
            {
                CouponLogPageCount = couponLogs.Item2,
                CouponLogs = couponLogs.Item1
            };
        }

        /// <summary>
        /// 文券记录总页数
        /// </summary>
        public int? CouponLogPageCount { get; set; }

        /// <summary>
        /// 文券记录列表
        /// </summary>
        public CouponLogList CouponLogs { get; set; }
    }
}