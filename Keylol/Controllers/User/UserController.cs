using System.Web.Http;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
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
        private readonly KeylolRoleManager _roleManager;

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
        /// <param name="roleManager"><see cref="KeylolRoleManager"/></param>
        public UserController(CouponProvider coupon, OwinContextProvider owinContextProvider, KeylolDbContext dbContext,
            KeylolUserManager userManager, OneTimeTokenProvider oneTimeToken, KeylolRoleManager roleManager)
        {
            _coupon = coupon;
            _owinContext = owinContextProvider.Current;
            _dbContext = dbContext;
            _userManager = userManager;
            _oneTimeToken = oneTimeToken;
            _roleManager = roleManager;
        }
    }
}