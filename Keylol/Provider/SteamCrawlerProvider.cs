using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.ServiceBase;
using Keylol.Services;
using Keylol.Utilities;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供 Steam 爬虫服务
    /// </summary>
    public class SteamCrawlerProvider
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CachedDataProvider.CachedDataProvider _cachedData;
        private readonly RedisProvider _redis;
        private static readonly string ApiKey = ConfigurationManager.AppSettings["steamWebApiKey"] ?? string.Empty;
        private static readonly HttpClient HttpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(20)};

        private static string UserSteamGameRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-game-records:{userId}";

        private static string UserSteamFriendRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-friend-records:{userId}";

        /// <summary>
        /// 创建 <see cref="SteamCrawlerProvider"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        public SteamCrawlerProvider(KeylolDbContext dbContext, KeylolUserManager userManager,
            CachedDataProvider.CachedDataProvider cachedData, RedisProvider redis)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
            _redis = redis;
        }

        /// <summary>
        /// 异步重新抓取指定用户的游戏记录 (fire-and-forget)
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public static void UpdateUserSteamGameRecords(string userId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                using (var userManager = new KeylolUserManager(dbContext))
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    await UpdateUserSteamGameRecordsAsync(userId, dbContext, userManager, redis,
                        new CachedDataProvider.CachedDataProvider(dbContext, redis));
                }
            });
        }

        /// <summary>
        /// 异步重新抓取指定用户的 Steam 好友列表 (fire-and-forget)
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public static void UpdateUserSteamFrineds(string userId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                using (var userManager = new KeylolUserManager(dbContext))
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    await UpdateUserSteamFrinedsAsync(userId, dbContext, userManager, redis);
                }
            });
        }

        /// <summary>
        /// 重新抓取指定用户的 Steam App 库
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>如果抓取成功，返回 <c>true</c></returns>
        public async Task<bool> UpdateUserSteamGameRecordsAsync(string userId)
        {
            return await UpdateUserSteamGameRecordsAsync(userId, _dbContext, _userManager, _redis, _cachedData);
        }

        private static async Task<bool> UpdateUserSteamGameRecordsAsync([NotNull] string userId,
            KeylolDbContext dbContext, KeylolUserManager userManager, RedisProvider redis,
            CachedDataProvider.CachedDataProvider cachedData)
        {
            var cacheKey = UserSteamGameRecordsCrawlerStampCacheKey(userId);
            var redisDb = redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return false;
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), TimeSpan.FromHours(12));
            try
            {
                var user = await userManager.FindByIdAsync(userId);
                var steamId = new SteamID();
                steamId.SetFromSteam3String(await userManager.GetSteamIdAsync(user.Id));
                string allGamesHtml;
                if (user.SteamBotId != null && user.SteamBot.IsOnline())
                {
                    var botCoordinator = SteamBotCoordinator.Sessions[user.SteamBot.SessionId];
                    allGamesHtml = await botCoordinator.Client.Curl(user.SteamBotId,
                        $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                }
                else
                {
                    allGamesHtml = await HttpClient.GetStringAsync(
                        $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                }
                if (string.IsNullOrWhiteSpace(allGamesHtml))
                    throw new Exception();
                var match = Regex.Match(allGamesHtml, @"<script language=""javascript"">\s*var rgGames = (.*)");
                if (!match.Success)
                    throw new Exception();
                var trimed = match.Groups[1].Value.Trim();
                var games = JArray.Parse(trimed.Substring(0, trimed.Length - 1));
                var oldRecords = (await dbContext.UserSteamGameRecords.Where(r => r.UserId == user.Id).ToListAsync())
                    .ToDictionary(r => r.SteamAppId, r => r);
                foreach (var game in games)
                {
                    var appId = (int) game["appid"];
                    UserSteamGameRecord record;
                    if (oldRecords.TryGetValue(appId, out record))
                    {
                        oldRecords.Remove(appId);
                    }
                    else
                    {
                        record = new UserSteamGameRecord
                        {
                            UserId = user.Id,
                            SteamAppId = appId
                        };
                        dbContext.UserSteamGameRecords.Add(record);
                    }
                    record.TwoWeekPlayedTime = game["hours"] != null ? (double) game["hours"] : 0;
                    record.TotalPlayedTime = game["hours_forever"] != null ? (double) game["hours_forever"] : 0;
                    if (game["last_played"] != null)
                        record.LastPlayTime = Helpers.DateTimeFromTimeStamp((ulong) game["last_played"]);
                }
                dbContext.UserSteamGameRecords.RemoveRange(oldRecords.Values);
                await dbContext.SaveChangesAsync();
                await cachedData.Users.PurgeSteamAppLibraryCacheAsync(userId);
                return true;
            }
            catch (Exception)
            {
                await redisDb.KeyDeleteAsync(cacheKey);
                return false;
            }
        }

        /// <summary>
        /// 重新抓取指定用户的 Steam 好友列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="redis"><see cref="RedisProvider"/></param>
        /// <returns>如果抓取成功，返回 <c>true</c></returns>
        public static async Task<bool> UpdateUserSteamFrinedsAsync([NotNull] string userId, KeylolDbContext dbContext,
            KeylolUserManager userManager, RedisProvider redis)
        {
            var cacheKey = UserSteamFriendRecordsCrawlerStampCacheKey(userId);
            var redisDb = redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return false;
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), TimeSpan.FromDays(2));
            try
            {
                var steamId = new SteamID();
                steamId.SetFromSteam3String(await userManager.GetSteamIdAsync(userId));
                var result = JObject.Parse(await HttpClient.GetStringAsync(
                    $"http://api.steampowered.com/ISteamUser/GetFriendList/v1/?key={ApiKey}&format=json&steamid={steamId.ConvertToUInt64()}&relationship=friend"));
                var oldRecords = (await dbContext.UserSteamFriendRecords.Where(r => r.UserId == userId).ToListAsync())
                    .ToDictionary(r => r.FriendSteamId, r => r);
                foreach (var friend in result["friendslist"]["friends"])
                {
                    var friendSteamId = (new SteamID(ulong.Parse((string) friend["steamid"]))).Render(true);
                    UserSteamFriendRecord record;
                    if (oldRecords.TryGetValue(friendSteamId, out record))
                    {
                        oldRecords.Remove(friendSteamId);
                    }
                    else
                    {
                        record = new UserSteamFriendRecord
                        {
                            UserId = userId,
                            FriendSteamId = friendSteamId
                        };
                        dbContext.UserSteamFriendRecords.Add(record);
                        if (friend["friend_since"] != null)
                            record.FriendSince = Helpers.DateTimeFromTimeStamp((ulong) friend["friend_since"]);
                    }
                }
                dbContext.UserSteamFriendRecords.RemoveRange(oldRecords.Values);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                await redisDb.KeyDeleteAsync(cacheKey);
                return false;
            }
        }
    }
}