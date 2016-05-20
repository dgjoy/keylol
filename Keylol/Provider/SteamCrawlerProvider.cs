using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
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
        private static readonly int SilenceSeconds = 8*60;

        private static string UserSteamGameRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-game-records:{userId}";

        private static string UserSteamFriendRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-friend-records:{userId}";

        private static string PointPriceCrawlerStampCacheKey(string pointId) => $"crawler-stamp:point-price:{pointId}";

        private static string PointSteamSpyCrawlerStampCacheKey(string pointId)
            => $"crawler-stamp:point-steam-spy:{pointId}";

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
        /// 异步抓取指定用户的游戏记录 (fire-and-forget)
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
        /// 异步抓取指定用户的 Steam 好友列表 (fire-and-forget)
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
        /// 抓取指定用户的 Steam App 库
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>如果抓取成功，返回 <c>true</c></returns>
        public async Task<bool> UpdateUserSteamGameRecordsAsync(string userId)
        {
            return await UpdateUserSteamGameRecordsAsync(userId, _dbContext, _userManager, _redis, _cachedData);
        }

        /// <summary>
        /// 异步抓取指定据点的价格 (fire-and-forget)
        /// </summary>
        /// <param name="pointId"></param>
        public static void UpdatePointPrice(string pointId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    await UpdatePointPriceAsync(pointId, dbContext, redis);
                }
            });
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
                await redisDb.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(SilenceSeconds));
                return false;
            }
        }

        private static async Task<bool> UpdateUserSteamFrinedsAsync([NotNull] string userId, KeylolDbContext dbContext,
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
                await redisDb.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(SilenceSeconds));
                return false;
            }
        }

        private static async Task<bool> UpdatePointPriceAsync([NotNull] string pointId, KeylolDbContext dbContext,
            RedisProvider redis)
        {
            var cacheKey = PointPriceCrawlerStampCacheKey(pointId);
            var redisDb = redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return false;
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), TimeSpan.FromDays(1));
            try
            {
                var point = await dbContext.Points.FindAsync(pointId);
                if (point.SteamAppId != null) // Steam
                {
                    var steamResult = JToken.Parse(await HttpClient.GetStringAsync(
                        $"http://store.steampowered.com/api/appdetails/?appids={point.SteamAppId}&cc=cn&l=english"));
                    steamResult = steamResult[point.SteamAppId.ToString()];
                    if ((bool) steamResult["success"])
                    {
                        steamResult = steamResult["data"];
                        if ((bool) steamResult["is_free"])
                        {
                            point.SteamPrice = 0;
                            point.SteamDiscountedPrice = null;
                        }
                        else
                        {
                            point.SteamPrice = (double) steamResult["price_overview"]["initial"]/100;
                            point.SteamDiscountedPrice = (int) steamResult["price_overview"]["discount_percent"] > 0
                                ? (double) steamResult["price_overview"]["final"]/100
                                : (double?) null;
                        }
                        await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
                    }
                }
                if (point.SonkwoProductId != null) // 杉果
                {
                    var sonkwoHtml =
                        await HttpClient.GetStringAsync($"http://www.sonkwo.com/products/{point.SonkwoProductId}");
                    var sonkwoDom = CQ.Create(sonkwoHtml);
                    var sonkwoPriceText = sonkwoDom[".sale-price"]?.Text().Replace("￥", string.Empty) ??
                                          sonkwoDom[".list-price"]?.Text().Replace("￥", string.Empty);
                    double sonkwoPrice;
                    if (double.TryParse(sonkwoPriceText, out sonkwoPrice))
                    {
                        point.SonkwoPrice = sonkwoPrice;
                        var sonkwoDiscountedPriceText = sonkwoDom[".discounted-sale-price"]?.Text()
                            .Replace("￥", string.Empty);
                        double sonkwoDiscountedPrice;
                        if (double.TryParse(sonkwoDiscountedPriceText, out sonkwoDiscountedPrice))
                        {
                            point.SonkwoDiscountedPrice = sonkwoDiscountedPrice;
                        }
                        else
                        {
                            point.SonkwoDiscountedPrice = null;
                        }
                        await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                await redisDb.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(SilenceSeconds));
                return false;
            }
        }

        private static async Task<bool> UpdateSteamSpyData(string pointId, KeylolDbContext dbContext,
            RedisProvider redis)
        {
            var cacheKey = PointSteamSpyCrawlerStampCacheKey(pointId);
            var redisDb = redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return false;
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), TimeSpan.FromDays(1));
            try
            {
                var point = await dbContext.Points.FindAsync(pointId);
                if (point.SteamAppId == null) return true;
                var result = JToken.Parse(await HttpClient.GetStringAsync(
                    $"http://steamspy.com/api.php?request=appdetails&appid={point.SteamAppId}"));
                if (result["name"] == null) return true;
                point.OwnerCount = (int) result["owners"];
                point.OwnerCountVariance = (int) result["owners_variance"];
                point.TotalPlayerCount = (int) result["players_forever"];
                point.TotalPlayerCountVariance = (int) result["players_forever_variance"];
                point.TwoWeekPlayerCount = (int) result["players_2weeks"];
                point.TwoWeekAveragePlayedTime = (int) result["players_2weeks_variance"];
                point.AveragePlayedTime = (int) result["average_forever"];
                point.TwoWeekAveragePlayedTime = (int) result["average_2weeks"];
                point.MedianPlayedTime = (int) result["median_forever"];
                point.TwoWeekMedianPlayedTime = (int) result["median_2weeks"];
                point.Ccu = (int) result["ccu"];
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
                return true;
            }
            catch (Exception)
            {
                await redisDb.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(SilenceSeconds));
                return false;
            }
        }
    }
}