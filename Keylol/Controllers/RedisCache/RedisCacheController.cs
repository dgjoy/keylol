using System.Web.Http;
using Keylol.Identity;
using Keylol.Provider;

namespace Keylol.Controllers.RedisCache
{
    /// <summary>
    ///     Redis 缓存 Controller
    /// </summary>
    [Authorize(Roles = KeylolRoles.Operator)]
    [RoutePrefix("redis-cache")]
    public partial class RedisCacheController : ApiController
    {
        private readonly RedisProvider _redis;

        /// <summary>
        ///     创建 <see cref="RedisCacheController" />
        /// </summary>
        /// <param name="redis">
        ///     <see cref="RedisProvider" />
        /// </param>
        public RedisCacheController(RedisProvider redis)
        {
            _redis = redis;
        }
    }
}