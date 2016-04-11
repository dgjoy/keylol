using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.UserPointSubscription
{
    public partial class UserPointSubscriptionController
    {
        /// <summary>
        ///     获取当前用户自动订阅的据点
        /// </summary>
        [Route("my/auto")]
        [HttpGet]
        public async Task<IHttpActionResult> GetListOfAutoByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            var subscriptions = await DbContext.AutoSubscriptions.Include(s => s.NormalPoint)
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
            return Ok(new
            {
                MostPlayed = subscriptions.Where(s => s.Type == AutoSubscriptionType.MostPlayed)
                    .Select(s => new NormalPointDto(s.NormalPoint)),
                RecentPlayed = subscriptions.Where(s => s.Type == AutoSubscriptionType.RecentPlayed)
                    .Select(s => new NormalPointDto(s.NormalPoint)),
                Genres = subscriptions.Where(s => s.Type == AutoSubscriptionType.Genre)
                    .Select(s => new NormalPointDto(s.NormalPoint)),
                Manufacturers = subscriptions.Where(s => s.Type == AutoSubscriptionType.Manufacture)
                    .Select(s => new NormalPointDto(s.NormalPoint))
            });
        }
    }
}