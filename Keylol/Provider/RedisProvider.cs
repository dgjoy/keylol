using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using StackExchange.Redis;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供 Redis 服务
    /// </summary>
    public static class RedisProvider
    {
        private static ConnectionMultiplexer _connectionMultiplexer;

        /// <summary>
        /// 获取全局 Redis 单例
        /// </summary>
        /// <returns>StackExchange.Redis Connection.Multiplexer</returns>
        public static ConnectionMultiplexer GetInstance()
        {
            if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
            {
                _connectionMultiplexer =
                    ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redisConnection"] ??
                                                  "localhost,abortConnect=false");
            }
            return _connectionMultiplexer;
        }

        /// <summary>
        /// 将对象序列化 BSON
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="object">要序列化的对象</param>
        /// <returns>序列化结果 BSON</returns>
        public static byte[] Serialize<T>(T @object)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer {Converters = {new StringEnumConverter()}};
                serializer.Serialize(writer, @object);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// 将 BSON 反序列化为对象
        /// </summary>
        /// <param name="data">要反序列化的 BSON</param>
        /// <param name="readRootValueAsArray">是否把 BSON 根看成数组</param>
        /// <returns>反序列化后的对象</returns>
        public static object Deserialize(byte[] data, bool readRootValueAsArray = false)
        {
            var ms = new MemoryStream(data);
            using (var reader = new BsonReader(ms) {ReadRootValueAsArray = readRootValueAsArray})
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }
    }
}