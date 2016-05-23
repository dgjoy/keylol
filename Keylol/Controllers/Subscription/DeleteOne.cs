using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Subscription
{
    public partial class SubscriptionController
    {
        /// <summary>
        /// 删除一个订阅
        /// </summary>
        /// <param name="targetId">目标 ID</param>
        /// <param name="targetType">目标类型</param>
        [Route]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteOne(string targetId, SubscriptionTargetType targetType)
        {
            var subscriberId = User.Identity.GetUserId();
            await _cachedData.Subscriptions.RemoveAsync(subscriberId, targetId, targetType);
            return Ok();
        }
    }
}