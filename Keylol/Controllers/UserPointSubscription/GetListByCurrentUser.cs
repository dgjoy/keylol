using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.UserPointSubscription
{
    public partial class UserPointSubscriptionController
    {
        /// <summary>
        ///     获取当前用户订阅的据点
        /// </summary>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("my")]
        [HttpGet]
        [ResponseType(typeof (List<SubscribedPointDTO>))]
        public async Task<IHttpActionResult> GetListByCurrentUser(int skip = 0, int take = 30)
        {
            if (take > 50) take = 50;
            var userId = User.Identity.GetUserId();
            var userQuery = DbContext.Users.AsNoTracking().Where(u => u.Id == userId);
            return Ok((await userQuery.SelectMany(u => u.SubscribedPoints.OfType<Models.NormalPoint>())
                .Select(p => new
                {
                    user = (KeylolUser) null,
                    userProfilePoint = (ProfilePoint) null,
                    point = p,
                    articleCount = p.Articles.Count,
                    subscriberCount = p.Subscribers.Count
                })
                .Concat(userQuery.SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>())
                    .Select(p => new
                    {
                        user = p.User,
                        userProfilePoint = p.User.ProfilePoint,
                        point = (Models.NormalPoint) null,
                        articleCount = p.Entries.OfType<Models.Article>().Count(),
                        subscriberCount = p.Subscribers.Count
                    }))
                .OrderBy(e => e.articleCount)
                .Skip(() => skip).Take(() => take)
                .ToListAsync()).Select(e => new SubscribedPointDTO
                {
                    User = e.user == null
                        ? null
                        : new UserDTO(e.user) {ProfilePointBackgroundImage = e.userProfilePoint.BackgroundImage},
                    NormalPoint = e.point == null ? null : new NormalPointDTO(e.point),
                    ArticleCount = e.articleCount,
                    SubscriberCount = e.subscriberCount
                }));
        }
    }
}