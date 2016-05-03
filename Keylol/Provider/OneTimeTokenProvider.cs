using System;
using System.Threading.Tasks;
using Keylol.ServiceBase;

namespace Keylol.Provider
{
    /// <summary>
    ///     提供 One-time Token 服务
    /// </summary>
    public class OneTimeTokenProvider
    {
        private readonly RedisProvider _redis;

        /// <summary>
        ///     创建 <see cref="OneTimeTokenProvider" />
        /// </summary>
        /// <param name="redis">
        ///     <see cref="RedisProvider" />
        /// </param>
        public OneTimeTokenProvider(RedisProvider redis)
        {
            _redis = redis;
        }

        private static string CacheKey(string token) => $"one-time-token:{token}";

        /// <summary>
        ///     生成一个 One-time Token
        /// </summary>
        /// <param name="payload">该 Token 对应的负载，可以在之后 Consume 时取出</param>
        /// <param name="expiry">Token 有效期</param>
        /// <typeparam name="T">负载类型</typeparam>
        /// <returns>生成的 One-time Token</returns>
        public async Task<string> Generate<T>(T payload, TimeSpan expiry)
        {
            var guid = Guid.NewGuid();
            var token = Helpers.Md5(guid.ToByteArray());
            var redisDb = _redis.GetDatabase();
            await redisDb.StringSetAsync(CacheKey(token), RedisProvider.Serialize(payload), expiry);
            return token;
        }

        /// <summary>
        ///     消耗一个 One-time Token，并使其失效
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="payloadIsArray">负载是否是数组类型</param>
        /// <typeparam name="T">负载类型</typeparam>
        /// <returns>如果 Token 有效返回 <c>true</c></returns>
        /// <exception cref="ArgumentNullException">token 参数为空</exception>
        /// <exception cref="InvalidOperationException">该 Token 无效</exception>
        public async Task<T> Consume<T>(string token, bool payloadIsArray = false)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            var cacheKey = CacheKey(token);
            var redisDb = _redis.GetDatabase();
            var result = await redisDb.StringGetAsync(cacheKey);
            if (result.IsNull)
                throw new InvalidOperationException("Invalid one-time token.");
            await redisDb.KeyDeleteAsync(cacheKey);
            return RedisProvider.Deserialize<T>(result, payloadIsArray);
        }
    }
}