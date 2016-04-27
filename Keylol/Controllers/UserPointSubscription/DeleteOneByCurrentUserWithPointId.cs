using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.UserPointSubscription
{
    public partial class UserPointSubscriptionController
    {
        /// <summary>
        ///     取消订阅指定据点或者用户
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        /// <param name="isAutoSubscription">指定据点是否是自动订阅的据点，默认 false</param>
        [Route]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NotFound, "当前用户并没有订阅指定据点或用户")]
        public async Task<IHttpActionResult> DeleteOneByCurrentUserWithPointId(string pointId,
            bool isAutoSubscription = false)
        {
            var userId = User.Identity.GetUserId();
            if (isAutoSubscription)
            {
                var subscription = await _dbContext.AutoSubscriptions
                    .Where(s => s.UserId == userId && s.NormalPointId == pointId)
                    .SingleOrDefaultAsync();
                if (subscription == null)
                    return NotFound();
                _dbContext.AutoSubscriptions.Remove(subscription);
            }
            else
            {
                var user =
                    await _dbContext.Users.Include(u => u.SubscribedPoints).SingleOrDefaultAsync(u => u.Id == userId);
                var point = await _dbContext.Points.FindAsync(pointId);
                if (point == null)
                    return NotFound();

                if (!user.SubscribedPoints.Contains(point))
                    return NotFound();
                user.SubscribedPoints.Remove(point);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}