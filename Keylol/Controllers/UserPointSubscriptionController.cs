using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("user-point-subscription")]
    public class UserPointSubscriptionController : KeylolApiController
    {
        /// <summary>
        /// 获取当前用户对指定据点或用户的订阅状态
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        [Route]
        [ResponseType(typeof (bool))]
        public async Task<IHttpActionResult> Get(string pointId)
        {
            var userId = User.Identity.GetUserId();
            return Ok(await DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.SubscribedPoints)
                .Select(p => p.Id)
                .ContainsAsync(pointId));
        }

        /// <summary>
        /// 订阅一个据点或者用户
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (string))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点或用户不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "尝试订阅自己，操作无效")]
        [SwaggerResponse(HttpStatusCode.Conflict, "用户已经订阅过该据点或用户")]
        public async Task<IHttpActionResult> Post(string pointId)
        {
            var point = await DbContext.Points.FindAsync(pointId);
            if (point == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (point.Id == userId)
                return Unauthorized();

            var user = await DbContext.Users.Include(u => u.SubscribedPoints).SingleOrDefaultAsync(u => u.Id == userId);
            if (user.SubscribedPoints.Contains(point))
                return Conflict();
            user.SubscribedPoints.Add(point);
            await DbContext.SaveChangesAsync();
            return Created($"user-point-subscription/{point.Id}", "Subscribed!");
        }

        /// <summary>
        /// 取消订阅指定据点或者用户
        /// </summary>
        /// <param name="pointId">据点 ID 或者用户 ID</param>
        [Route]
        [SwaggerResponse(HttpStatusCode.NotFound, "当前用户并没有订阅指定据点或用户")]
        public async Task<IHttpActionResult> Delete(string pointId)
        {
            var userId = User.Identity.GetUserId();
            var user = await DbContext.Users.Include(u => u.SubscribedPoints).SingleOrDefaultAsync(u => u.Id == userId);
            var point = await DbContext.Points.FindAsync(pointId);
            if (point == null)
                return NotFound();

            if (!user.SubscribedPoints.Contains(point))
                return NotFound();
            user.SubscribedPoints.Remove(point);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}