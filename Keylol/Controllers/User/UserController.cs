using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Microsoft.Owin;
using SimpleInjector.Integration.Owin;

namespace Keylol.Controllers.User
{
    /// <summary>
    ///     用户 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("user")]
    public partial class UserController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;
        private readonly OneTimeTokenProvider _oneTimeToken;
        private readonly IOwinContext _owinContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="UserController" />
        /// </summary>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="owinContextProvider">
        ///     <see cref="OwinContextProvider" />
        /// </param>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        /// <param name="oneTimeToken">
        ///     <see cref="OneTimeTokenProvider" />
        /// </param>
        public UserController(CouponProvider coupon, OwinContextProvider owinContextProvider, KeylolDbContext dbContext,
            KeylolUserManager userManager, OneTimeTokenProvider oneTimeToken)
        {
            _coupon = coupon;
            _dbContext = dbContext;
            _userManager = userManager;
            _oneTimeToken = oneTimeToken;
            _owinContext = owinContextProvider.Current;
        }
    }
}