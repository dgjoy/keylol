using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.Controllers.Like
{
    /// <summary>
    /// 认可 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("like")]
    public partial class LikeController : KeylolApiController
    {
        private readonly CouponProvider _coupon;
        private readonly StatisticsProvider _statistics;

        /// <summary>
        /// 认可类型
        /// </summary>
        public enum LikeType
        {
            /// <summary>
            /// 文章认可
            /// </summary>
            ArticleLike,

            /// <summary>
            /// 评论认可
            /// </summary>
            CommentLike
        }

        /// <summary>
        /// 创建 <see cref="LikeController"/>
        /// </summary>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="statistics"><see cref="StatisticsProvider"/></param>
        public LikeController(CouponProvider coupon, StatisticsProvider statistics)
        {
            _coupon = coupon;
            _statistics = statistics;
        }
    }
}