using Keylol.States.Coupon.Detail;
using Keylol.States.Coupon.Ranking;

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
    }
}