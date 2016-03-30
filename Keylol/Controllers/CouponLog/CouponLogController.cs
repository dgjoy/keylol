using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Keylol.Controllers.CouponLog
{
    /// <summary>
    /// 文券日志 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("coupon-log")]
    public partial class CouponLogController : KeylolApiController
    {
    }
}