using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     获取当前用户的读者
        /// </summary>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("my")]
        [HttpGet]
        [ResponseType(typeof (List<UserDto>))]
        public async Task<IHttpActionResult> GetListOfSubscribersByCurrentUser(int skip = 0, int take = 30)
        {
            if (take > 50) take = 50;
            var userId = User.Identity.GetUserId();
            return Ok((await DbContext.Users.AsNoTracking().Where(u => u.Id == userId)
                .SelectMany(u => u.ProfilePoint.Subscribers)
                .Select(u => new
                {
                    user = u,
                    profilePoint = u.ProfilePoint,
                    articleCount = u.ProfilePoint.Articles.Count(),
                    subscriberCount = u.ProfilePoint.Subscribers.Count
                })
                .OrderBy(e => e.user.RegisterTime)
                .Skip(() => skip).Take(() => take)
                .ToListAsync())
                .Select(entry => new UserDto(entry.user)
                {
                    ProfilePointBackgroundImage = entry.profilePoint.BackgroundImage,
                    ArticleCount = entry.articleCount,
                    SubscriberCount = entry.subscriberCount
                }));
        }
    }
}