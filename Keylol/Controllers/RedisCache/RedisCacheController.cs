using System.Web.Http;
using Keylol.Provider;
using Keylol.Utilities;

namespace Keylol.Controllers.RedisCache
{
    /// <summary>
    /// Redis 缓存 Controller
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("redis")]
    public partial class RedisCacheController : KeylolApiController
    {
        private readonly RedisProvider _redis;

        /// <summary>
        /// 创建 <see cref="RedisCacheController"/>
        /// </summary>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public RedisCacheController(RedisProvider redis)
        {
            _redis = redis;
        }
    }
}