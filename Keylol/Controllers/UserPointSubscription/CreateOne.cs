using System.Data.Entity;
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
        ///     订阅一个据点或者用户
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点或用户不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "尝试订阅自己，操作无效")]
        [SwaggerResponse(HttpStatusCode.Conflict, "用户已经订阅过该据点或用户")]
        public async Task<IHttpActionResult> CreateOne(string pointId)
        {
            var point = await _dbContext.Points.FindAsync(pointId);
            if (point == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (point.Id == userId)
                return Unauthorized();

            var user = await _dbContext.Users.Include(u => u.SubscribedPoints).SingleOrDefaultAsync(u => u.Id == userId);
            if (user.SubscribedPoints.Contains(point))
                return Conflict();
            user.SubscribedPoints.Add(point);
            await _dbContext.SaveChangesAsync();
            return Created($"user-point-subscription/{point.Id}", "Subscribed!");
        }
    }
}