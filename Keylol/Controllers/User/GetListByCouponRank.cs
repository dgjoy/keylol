using System.Threading.Tasks;
using System.Web.Http;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        /// 获取文券排行榜的用户列表
        /// </summary>
        [Route("coupon-rank")]
        [HttpGet]
        public async Task<IHttpActionResult> GetListByCouponRank()
        {
            return Ok();
        }
    }
}