using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
using RabbitMQ.Client;

namespace Keylol.Controllers.Like
{
    /// <summary>
    ///     认可 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("like")]
    public partial class LikeController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CachedDataProvider _cachedData;
        private readonly IModel _mqChannel;

        /// <summary>
        ///     创建 <see cref="LikeController" />
        /// </summary>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        /// <param name="cachedData">
        ///     <see cref="CachedDataProvider"/>
        /// </param>
        /// <param name="mqChannel"><see cref="IModel"/></param>
        public LikeController(CouponProvider coupon, KeylolDbContext dbContext,
            KeylolUserManager userManager, CachedDataProvider cachedData, IModel mqChannel)
        {
            _coupon = coupon;
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
            _mqChannel = mqChannel;
        }
    }
}