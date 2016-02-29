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
    public static class RedisProvider
    {
        private static ConnectionMultiplexer _connectionMultiplexer;

        public static ConnectionMultiplexer GetInstance()
        {
            if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
            {
                _connectionMultiplexer =
                    ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redisConnection"]);
            }
            return _connectionMultiplexer;
        }

        public static async Task<bool> Set(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = 0,
            CommandFlags flags = 0)
        {
            try
            {
                return await GetInstance().GetDatabase().StringSetAsync(key, value, expiry, when, flags);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<RedisValue> Get(RedisKey key, CommandFlags flags = 0)
        {
            try
            {
                return await GetInstance().GetDatabase().StringGetAsync(key, flags);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static async Task<bool> Delete(RedisKey key, CommandFlags flags = 0)
        {
            try
            {
                return await GetInstance().GetDatabase().KeyDeleteAsync(key, flags);
            }
            catch (Exception)
            {
                return false;
            }
        }

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

        public static object Deserialize(byte[] data, bool readRootValueAsArray = false)
        {
            var ms = new MemoryStream(data);
            using (var reader = new BsonReader(ms) {ReadRootValueAsArray = readRootValueAsArray})
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }

        public static void BackgroundJob(Action action)
        {
            Task.Run(action);
        }
    }
}