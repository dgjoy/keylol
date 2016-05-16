using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.ServiceBase;
using Keylol.Services;
using Keylol.Utilities;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供用户游戏记录操作服务
    /// </summary>
    public class UserGameRecordProvider
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;
        private readonly CachedDataProvider.CachedDataProvider _cachedData;

        /// <summary>
        /// 创建 <see cref="UserGameRecordProvider"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public UserGameRecordProvider(KeylolDbContext dbContext, KeylolUserManager userManager,
            CachedDataProvider.CachedDataProvider cachedData)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _cachedData = cachedData;
        }

        /// <summary>
        /// 异步重新抓取指定用户的游戏记录 (fire-and-forget)
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public static void UpdateUser(string userId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                using (var userManager = new KeylolUserManager(dbContext))
                {
                    await UpdateUserAsync(userId, dbContext, userManager,
                        new CachedDataProvider.CachedDataProvider(dbContext,
                            Global.Container.GetInstance<RedisProvider>()));
                }
            });
        }

        /// <summary>
        /// 重新抓取指定用户的游戏记录
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>如果抓取成功，返回 <c>true</c></returns>
        public async Task<bool> UpdateUserAsync(string userId)
        {
            return await UpdateUserAsync(userId, _dbContext, _userManager, _cachedData);
        }

        private static async Task<bool> UpdateUserAsync(string userId, KeylolDbContext dbContext,
            KeylolUserManager userManager, CachedDataProvider.CachedDataProvider cachedData)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user.LastGameUpdateSucceed && DateTime.Now - user.LastGameUpdateTime < TimeSpan.FromDays(3))
                return false;

            user.LastGameUpdateTime = DateTime.Now;
            user.LastGameUpdateSucceed = true;
            await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            try
            {
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
                    var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(20)};
                    allGamesHtml = await httpClient.GetStringAsync(
                        $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                }
                if (string.IsNullOrWhiteSpace(allGamesHtml))
                    throw new Exception();
                var match = Regex.Match(allGamesHtml, @"<script language=""javascript"">\s*var rgGames = (.*)");
                if (!match.Success)
                    throw new Exception();
                var trimed = match.Groups[1].Value.Trim();
                var games = JArray.Parse(trimed.Substring(0, trimed.Length - 1));
                foreach (var game in games)
                {
                    var appId = (int) game["appid"];

                    var record = await dbContext.UserGameRecords
                        .Where(r => r.UserId == user.Id && r.SteamAppId == appId)
                        .SingleOrDefaultAsync();
                    if (record == null)
                    {
                        record = new UserGameRecord
                        {
                            UserId = user.Id,
                            SteamAppId = appId
                        };
                        dbContext.UserGameRecords.Add(record);
                    }
                    record.TwoWeekPlayedTime = game["hours"] != null ? (double) game["hours"] : 0;
                    record.TotalPlayedTime = game["hours_forever"] != null ? (double) game["hours_forever"] : 0;
                    if (game["last_played"] != null)
                        record.LastPlayTime = Helpers.DateTimeFromTimeStamp((int) game["last_played"]);
                }
                await dbContext.SaveChangesAsync();
                await cachedData.Users.PurgeSteamAppLibraryCacheAsync(userId);
                return true;
            }
            catch (Exception)
            {
                user.LastGameUpdateSucceed = false;
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
                return false;
            }
        }
    }
}