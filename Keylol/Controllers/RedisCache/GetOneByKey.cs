using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.RedisCache
{
    public partial class RedisCacheController
    {
        /// <summary>
        ///     获取指定 Key 的缓存值
        /// </summary>
        /// <param name="key">要获取的 Key</param>
        [Route]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定 Key 不存在")]
        public async Task<IHttpActionResult> GetOneByKey(string key)
        {
            var redisDb = _redis.GetDatabase();
            if (!await redisDb.KeyExistsAsync(key))
                return NotFound();
            return Ok(await redisDb.StringGetAsync(key));
        }
    }
}