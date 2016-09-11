using Keylol.States.Coupon.Detail;
using Keylol.States.Coupon.Ranking;
using Keylol.States.Coupon.Store;

namespace Keylol.States.Coupon
{
    /// <summary>
    /// 文券层级
    /// </summary>
    public class CouponLevel
    {
        /// <summary>
        /// 明细
        /// </summary>
        public DetailPage Detail { get; set; }

        /// <summary>
        /// 排行
        /// </summary>
        public RankingPage Ranking { get; set; }

        /// <summary>
        /// 商店
        /// </summary>
        public StorePage Store { get; set; }
    }
}