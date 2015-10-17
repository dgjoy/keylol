using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers
{
    [Authorize]
    [Route("user-point-subscription")]
    public class UserPointSubscriptionController : KeylolApiController
    {
        public async Task<IHttpActionResult> Post(string pointId)
        {
            var point = await DbContext.Points.FindAsync(pointId);
            if (point == null)
                return NotFound();

            var userId = User.Identity.GetUserId();
            if (point.Id == userId)
            {
                ModelState.AddModelError("pointId", "Cannot subscribe yourself.");
                return BadRequest(ModelState);
            }

            var user = await UserManager.FindByIdAsync(userId);
            user.SubscribedPoints.Add(point);
            await DbContext.SaveChangesAsync();
            return Created($"user-point-subscription/{point.Id}", "Subscribed!");
        }

        public async Task<IHttpActionResult> Delete(string pointId)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var point = await DbContext.Points.FindAsync(pointId);
            if (point == null)
                return NotFound();

            user.SubscribedPoints.Remove(point);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
