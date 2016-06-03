using System;
using System.Collections.Generic;
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
        private static readonly TimeSpan SilenceTime = TimeSpan.FromMinutes(8);
        private static readonly TimeSpan PointPriceUpdatePeriod = TimeSpan.FromDays(1);
        private static readonly TimeSpan UserSteamGameRecordsUpdatePeriod = TimeSpan.FromHours(12);
        private static readonly TimeSpan UserSteamFriendsUpdatePeriod = TimeSpan.FromDays(2);
        private static readonly TimeSpan SteamSpyDataUpdatePeriod = TimeSpan.FromDays(1);
        private static readonly TimeSpan OnSalePointsUpdateTime = TimeSpan.FromHours(3); // 凌晨三时
        private const int OnSalePointMinCount = 50; // 至少获取的是日优惠据点个数

        private static string UserSteamGameRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-game-records:{userId}";

        private static string UserSteamFriendRecordsCrawlerStampCacheKey(string userId)
            => $"crawler-stamp:user-steam-friend-records:{userId}";

        private static string PointPriceCrawlerStampCacheKey(string pointId) => $"crawler-stamp:point-price:{pointId}";

        private static string PointSteamSpyCrawlerStampCacheKey(string pointId)
            => $"crawler-stamp:point-steam-spy:{pointId}";

        private static string OnSalePointsCrawlerStampCacheKey() => "crawler-stamp:on-sale-points";

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
        /// <param name="pointId">据点 ID</param>
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

        /// <summary>
        /// 异步抓取指定据点的 SteamSpy 数据 (fire-and-forget)
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        public static void UpdateSteamSpyData(string pointId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    await UpdateSteamSpyData(pointId, dbContext, redis);
                }
            });
        }

        /// <summary>
        /// 异步抓取是日优惠据点 (fire-and-forget)
        /// </summary>
        public static void UpdateOnSalePoints()
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    await UpdateOnSalePoints(dbContext, redis);
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
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), UserSteamGameRecordsUpdatePeriod);
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
                await redisDb.KeyExpireAsync(cacheKey, SilenceTime);
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
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), UserSteamFriendsUpdatePeriod);
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
                await redisDb.KeyExpireAsync(cacheKey, SilenceTime);
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
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), PointPriceUpdatePeriod);
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
                    var sonkwoPriceText = sonkwoDom[".sale-price, .list-price"].Text()
                        .Replace("￥", string.Empty);
                    double sonkwoPrice;
                    if (double.TryParse(sonkwoPriceText, out sonkwoPrice))
                    {
                        point.SonkwoPrice = sonkwoPrice;
                        var sonkwoDiscountedPriceText = sonkwoDom[".discounted-sale-price"].Text()
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
                await redisDb.KeyExpireAsync(cacheKey, SilenceTime);
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
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), SteamSpyDataUpdatePeriod);
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
                await redisDb.KeyExpireAsync(cacheKey, SilenceTime);
                return false;
            }
        }

        private static async Task<bool> UpdateOnSalePoints(KeylolDbContext dbContext, RedisProvider redis)
        {
            var cacheKey = OnSalePointsCrawlerStampCacheKey();
            var redisDb = redis.GetDatabase();
            var cacheResult = await redisDb.StringGetAsync(cacheKey);
            if (cacheResult.HasValue)
                return false;
            var expiry = OnSalePointsUpdateTime - DateTime.Now.TimeOfDay;
            if (expiry <= TimeSpan.Zero)
                expiry += TimeSpan.FromHours(24);
            await redisDb.StringSetAsync(cacheKey, DateTime.Now.ToTimestamp(), expiry);
            try
            {
                var points = new HashSet<string>();
                var currentPage = 1;
                var continueNext = true;
                do
                {
                    var dom = CQ.Create(await HttpClient.GetStreamAsync(
                        $"http://store.steampowered.com/search/results?specials=1&os=win&page={currentPage}&cc=cn"));
                    var anchors = dom[".search_result_row"];
                    if (anchors.Length < 25)
                        continueNext = false;
                    foreach (var anchor in anchors)
                    {
                        int appId;
                        if (!int.TryParse(anchor.Attributes["data-ds-appid"], out appId)) continue;
                        var point = await dbContext.Points.Where(p => p.SteamAppId == appId).SingleOrDefaultAsync();
                        if (point == null) continue;
                        if (!points.Add(point.Id)) continue;
                        var matches = Regex.Matches(anchor.Cq().Find(".search_price").Text(), @"¥ (\d+)");
                        if (matches.Count != 2) continue;
                        point.SteamPrice = double.Parse(matches[0].Groups[1].Value);
                        point.SteamDiscountedPrice = double.Parse(matches[1].Groups[1].Value);
                    }
                    currentPage++;
                } while (continueNext && points.Count < OnSalePointMinCount);

                var oldPoints = await dbContext.Feeds.Where(f => f.StreamName == OnSalePointStream.Name).ToListAsync();
                dbContext.Feeds.RemoveRange(oldPoints);
                dbContext.Feeds.AddRange(points.Select(id => new Feed
                {
                    StreamName = OnSalePointStream.Name,
                    EntryType = FeedEntryType.PointId,
                    Entry = id
                }).Reverse());
                foreach (var pointId in points)
                {
                    await redisDb.StringSetAsync(PointPriceCrawlerStampCacheKey(pointId), DateTime.Now.ToTimestamp(),
                        PointPriceUpdatePeriod);
                }
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.DatabaseWin);
                return true;
            }
            catch (Exception)
            {
                await redisDb.KeyExpireAsync(cacheKey, SilenceTime);
                return false;
            }
        }
    }
}