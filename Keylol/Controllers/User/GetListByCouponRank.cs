using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        /// 获取文券排行榜的用户列表，并在 HTTP Header 中设置 X-My-Rank 值表示自己的排名（0 表示第 100 名以后）
        /// </summary>
        /// <param name="skip"> 起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 20</param>
        [Route("coupon-rank")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetListByCouponRank(int skip = 0, int take = 20)
        {
            var userId = User.Identity.GetUserId();
            var topUsers = await DbContext.Users.OrderByDescending(u => u.Coupon).Take(() => 100).ToListAsync();
            var myRank = topUsers.Select(u => u.Id).ToList().IndexOf(userId) + 1;
            var result = new List<UserDto>(take);
            foreach (var topUser in topUsers.Skip(skip).Take(take))
            {
                var dto = new UserDto(topUser)
                {
                    Coupon = topUser.Coupon,
                    LikeCount = await _statistics.GetUserLikeCount(topUser.Id)
                };
                result.Add(dto);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            response.Headers.Add("X-My-Rank", myRank.ToString());
            return response;
        }
    }
}