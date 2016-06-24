using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.UserPointSubscription
{
    public partial class UserPointSubscriptionController
    {
        /// <summary>
        ///     获取当前用户对指定据点或用户的订阅状态
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        [Route]
        [HttpGet]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> GetOneByCurrentUserWithPointId(string pointId)
        {
            var userId = User.Identity.GetUserId();
            return Ok(await _dbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.SubscribedPoints)
                .Select(p => p.Id)
                .ContainsAsync(pointId));
        }
    }
}