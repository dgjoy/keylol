using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.CouponLog
{
    public partial class CouponLogController
    {
        /// <summary>
        ///     获取当前用户未读的文券变动记录
        /// </summary>
        [Route("unread")]
        [HttpGet]
        [ResponseType(typeof (List<CouponLogDto>))]
        public async Task<IHttpActionResult> GetListOfUnreadByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            var logs =
                (await _coupon.PopUnreadCouponLogs(userId)).Select((dto, i) => new {Order = i, Log = dto}).ToList();
            var result = new List<CouponLogDto>();
            var first = logs.FirstOrDefault();
            var balance = first?.Log.Balance - first?.Log.Change ?? 0;
            foreach (var group in logs.GroupBy(l => l.Log.Event)
                .Select(g => new {Order = g.Average(l => l.Order), Group = g})
                .OrderBy(e => e.Order)
                .Select(e => e.Group))
            {
                var change = group.Sum(log => log.Log.Change ?? 0);
                result.Add(new CouponLogDto
                {
                    Event = group.Key,
                    Change = change,
                    Before = balance
                });
                balance += change;
            }
            return Ok(result);
        }
    }
}