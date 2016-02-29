using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Services;
using Keylol.Services.Contracts;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using SteamKit2;
using Swashbuckle.Swagger.Annotations;
using Extensions = Keylol.Utilities.Extensions;

namespace Keylol.Controllers.UserGameRecord
{
    public partial class UserGameRecordController
    {
        [Route("my")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "距离上次抓取不足最小抓取周期，或者网络问题导致抓取失败")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "用户资料设定为隐私，抓取失败")]
        public async Task<IHttpActionResult> UpdateListByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            var user = await DbContext.Users.Where(u => u.Id == userId).SingleAsync();
            if (DateTime.Now - user.LastGameUpdateTime < TimeSpan.FromDays(1) && user.LastGameUpdateSucceed)
                return NotFound();

            if (user.SteamBot.Enabled)
            {
                user.LastGameUpdateTime = DateTime.Now;
                user.LastGameUpdateSucceed = true;
                await DbContext.SaveChangesAsync();
                try
                {
                    var steamId = new SteamID();
                    steamId.SetFromSteam3String(user.SteamId);
                    ISteamBotCoodinatorCallback callback;
                    string allGamesHtml;
                    if (user.SteamBot.Online &&
                        SteamBotCoodinator.Clients.TryGetValue(user.SteamBot.SessionId, out callback))
                    {
                        allGamesHtml = callback.FetchUrl(user.SteamBotId,
                            $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                    }
                    else
                    {
                        var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(5)};
                        allGamesHtml = await httpClient.GetStringAsync(
                            $"http://steamcommunity.com/profiles/{steamId.ConvertToUInt64()}/games/?tab=all&l=english");
                    }
                    if (!string.IsNullOrEmpty(allGamesHtml))
                    {
                        var match = Regex.Match(allGamesHtml, @"<script language=""javascript"">\s*var rgGames = (.*)");
                        if (match.Success)
                        {
                            var trimed = match.Groups[1].Value.Trim();
                            var games =
                                JArray.Parse(trimed.Substring(0, trimed.Length - 1));
                            foreach (var game in games)
                            {
                                var appId = (int) game["appid"];
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
                                if (game["hours_forever"] != null)
                                    record.TotalPlayedTime = (double) game["hours_forever"];
                                if (game["last_played"] != null)
                                    record.LastPlayTime =
                                        Extensions.DateTimeFromUnixTimeStamp((int) game["last_played"]);
                                await DbContext.SaveChangesAsync();
                            }
                            return Ok();
                        }
                        if (Regex.IsMatch(allGamesHtml, @"This profile is private\."))
                            return Unauthorized();
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    user.LastGameUpdateSucceed = false;
                    await DbContext.SaveChangesAsync();
                }
            }
            return NotFound();
        }
    }
}