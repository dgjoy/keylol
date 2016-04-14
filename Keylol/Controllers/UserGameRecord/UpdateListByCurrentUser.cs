using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using SteamKit2;
using Swashbuckle.Swagger.Annotations;
using Extensions = Keylol.Utilities.Extensions;

namespace Keylol.Controllers.UserGameRecord
{
    public partial class UserGameRecordController
    {
        /// <summary>
        ///     重新抓取当前登录用户的游戏记录，并重新同步订阅
        /// </summary>
        /// <param name="manual">是否是用户手动触发的同步，默认 false</param>
        [Route("my")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "距离上次抓取不足最小抓取周期，或者网络问题导致抓取失败")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "用户资料设定为隐私，抓取失败")]
        public async Task<IHttpActionResult> UpdateListByCurrentUser(bool manual = false)
        {
            var userId = User.Identity.GetUserId();
            var user = await DbContext.Users.Where(u => u.Id == userId).SingleAsync();
            if (manual)
            {
                if (user.LastGameUpdateSucceed || DateTime.Now - user.LastGameUpdateTime < TimeSpan.FromMinutes(1))
                    return NotFound();
            }
            else
            {
                if (!user.AutoSubscribeEnabled ||
                    (DateTime.Now - user.LastGameUpdateTime < TimeSpan.FromDays(user.AutoSubscribeDaySpan) &&
                     user.LastGameUpdateSucceed))
                    return NotFound();
            }

            user.LastGameUpdateTime = DateTime.Now;
            user.LastGameUpdateSucceed = true;
            await DbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            try
            {
                var steamId = new SteamID();
                steamId.SetFromSteam3String(user.SteamId);
                string allGamesHtml;
                if (user.SteamBot.IsOnline())
                {
                    var botCoordinator = SteamBotCoordinator.Sessions[user.SteamBot.SessionId];
                    allGamesHtml = await botCoordinator.Client.Curl(user.SteamBotId,
                        $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                }
                else
                {
                    var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};
                    allGamesHtml = await httpClient.GetStringAsync(
                        $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                }
                if (!string.IsNullOrEmpty(allGamesHtml))
                {
                    var match = Regex.Match(allGamesHtml, @"<script language=""javascript"">\s*var rgGames = (.*)");
                    if (match.Success)
                    {
                        var trimed = match.Groups[1].Value.Trim();
                        var games = JArray.Parse(trimed.Substring(0, trimed.Length - 1));
                        foreach (var game in games)
                        {
                            var appId = (int) game["appid"];

                            double totalPlayedTime = 0;
                            if (game["hours_forever"] != null)
                                totalPlayedTime = (double) game["hours_forever"];
                            if (Math.Abs(totalPlayedTime) <= 0.5)
                                continue;

                            var record = await DbContext.UserGameRecords
                                .Where(r => r.UserId == userId && r.SteamAppId == appId)
                                .SingleOrDefaultAsync();
                            if (record == null)
                            {
                                record = DbContext.UserGameRecords.Create();
                                record.UserId = userId;
                                record.SteamAppId = appId;
                                DbContext.UserGameRecords.Add(record);
                            }
                            record.TotalPlayedTime = totalPlayedTime;

                            if (game["last_played"] != null)
                                record.LastPlayTime =
                                    Extensions.DateTimeFromUnixTimeStamp((int) game["last_played"]);
                        }
                        await DbContext.SaveChangesAsync();
                        var gameEntries = await DbContext.UserGameRecords
                            .Where(r => r.UserId == userId)
                            .OrderByDescending(r => r.TotalPlayedTime)
                            .Select(r =>
                                new
                                {
                                    record = r,
                                    point = DbContext.NormalPoints.FirstOrDefault(
                                        p => p.Type == NormalPointType.Game && p.SteamAppId == r.SteamAppId)
                                })
                            .Where(e => e.point != null)
                            .ToListAsync();
                        var mostPlayed = gameEntries.Where(g =>
                            !user.SubscribedPoints.OfType<Models.NormalPoint>().Contains(g.point))
                            .Select(g => g.point).Take(6).ToList();
                        var recentPlayed = gameEntries.Where(g =>
                            !user.SubscribedPoints.OfType<Models.NormalPoint>().Contains(g.point) &&
                            !mostPlayed.Contains(g.point))
                            .OrderByDescending(g => g.record.LastPlayTime)
                            .Select(g => g.point).Take(3).ToList();
                        var genreStats = new Dictionary<Models.NormalPoint, int>();
                        var manufacturerStats = new Dictionary<Models.NormalPoint, int>();
                        foreach (var game in gameEntries)
                        {
                            foreach (var point in game.point.TagPoints
                                .Concat(game.point.SeriesPoints)
                                .Concat(game.point.GenrePoints)
                                .Where(p => !user.SubscribedPoints.OfType<Models.NormalPoint>().Contains(p)))
                            {
                                if (genreStats.ContainsKey(point))
                                    genreStats[point]++;
                                else
                                    genreStats[point] = 1;
                            }
                            foreach (var point in game.point.DeveloperPoints
                                .Concat(game.point.PublisherPoints)
                                .Where(p => !user.SubscribedPoints.OfType<Models.NormalPoint>().Contains(p)))
                            {
                                if (manufacturerStats.ContainsKey(point))
                                    manufacturerStats[point]++;
                                else
                                    manufacturerStats[point] = 1;
                            }
                        }
                        var genres = genreStats.OrderByDescending(pair => pair.Value)
                            .Select(pair => pair.Key).Take(3).ToList();
                        var manufactures = manufacturerStats.OrderByDescending(pair => pair.Value)
                            .Select(pair => pair.Key).Take(3).ToList();
                        DbContext.AutoSubscriptions.RemoveRange(
                            await DbContext.AutoSubscriptions.Where(s => s.UserId == userId).ToListAsync());
                        var i = 0;
                        DbContext.AutoSubscriptions.AddRange(
                            mostPlayed.Select(p => new {p, t = AutoSubscriptionType.MostPlayed})
                                .Concat(recentPlayed.Select(p => new {p, t = AutoSubscriptionType.RecentPlayed}))
                                .Concat(genres.Select(p => new {p, t = AutoSubscriptionType.Genre}))
                                .Concat(manufactures.Select(p => new {p, t = AutoSubscriptionType.Manufacture}))
                                .Select(e =>
                                {
                                    var subscription = DbContext.AutoSubscriptions.Create();
                                    subscription.UserId = userId;
                                    subscription.NormalPointId = e.p.Id;
                                    subscription.Type = e.t;
                                    subscription.DisplayOrder = i;
                                    i++;
                                    return subscription;
                                }));
                        await DbContext.SaveChangesAsync();
                        return Ok(new
                        {
                            MostPlayed = mostPlayed.Select(p => new NormalPointDto(p)),
                            RecentPlayed = recentPlayed.Select(p => new NormalPointDto(p)),
                            Genres = genres.Select(p => new NormalPointDto(p)),
                            Manufacturers = manufactures.Select(p => new NormalPointDto(p))
                        });
                    }
                    if (Regex.IsMatch(allGamesHtml, @"This profile is private\."))
                        return Unauthorized();
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                user.LastGameUpdateSucceed = false;
                await DbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            }
            return NotFound();
        }
    }
}