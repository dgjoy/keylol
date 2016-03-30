using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Keylol.Controllers.RedisCache
{
    public partial class RedisCacheController
    {
        /// <summary>
        /// 删除匹配指定模式的缓存项
        /// </summary>
        /// <param name="pattern">匹配模式，如果为空表示清空全部缓存，默认 null</param>
        [Route]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteListByKeyPattern(string pattern = null)
        {
            var redisDb = _redis.GetDatabase();
            foreach (var server in _redis.Connection.GetEndPoints()
                .Select(endPoint => _redis.Connection.GetServer(endPoint)))
            {
                if (string.IsNullOrEmpty(pattern))
                    await server.FlushAllDatabasesAsync();
                else
                    await redisDb.KeyDeleteAsync(server.Keys(pattern: pattern).ToArray());
            }
            return Ok();
        }
    }
}