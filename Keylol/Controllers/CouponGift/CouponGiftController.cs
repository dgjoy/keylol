using System.Web.Http;
using Keylol.Provider;

namespace Keylol.Controllers.CouponGift
{
    /// <summary>
    /// 文券礼品 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-gift")]
    public partial class CouponGiftController : KeylolApiController
    {
    }
}