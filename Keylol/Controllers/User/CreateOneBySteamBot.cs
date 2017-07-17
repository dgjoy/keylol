using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Provider.CachedDataProvider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     通过 steam 机器人注册一个新用户
        /// </summary>
        /// <param name="bySteamBotRequestDto">用户相关属性</param>
        [AllowAnonymous]
        [Route("user/steambot")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOneBySteamBot([NotNull] UserCreateOneBySteamBotRequestDto bySteamBotRequestDto)
        {
            var steamBindingToken = await _dbContext.SteamBindingTokens.FindAsync(bySteamBotRequestDto.SteamBindingTokenId);

            if (steamBindingToken == null)
                return this.BadRequest(nameof(bySteamBotRequestDto), nameof(bySteamBotRequestDto.SteamBindingTokenId), Errors.Invalid);

            if (await _userManager.FindBySteamIdAsync(steamBindingToken.SteamId) != null)
                return this.BadRequest(nameof(bySteamBotRequestDto), nameof(bySteamBotRequestDto.SteamBindingTokenId), Errors.Duplicate);

            if (bySteamBotRequestDto.Email != null && (!new EmailAddressAttribute().IsValid(bySteamBotRequestDto.Email) ||
                                             await _userManager.FindByEmailAsync(bySteamBotRequestDto.Email) != null))
                bySteamBotRequestDto.Email = null;

            var user = new KeylolUser
            {
                IdCode = bySteamBotRequestDto.IdCode,
                UserName = bySteamBotRequestDto.UserName,
                Email = bySteamBotRequestDto.Email,
                RegisterIp = _owinContext.Request.RemoteIpAddress,
                SteamBindingTime = DateTime.Now,
                SteamBotId = steamBindingToken.BotId
            };

            if (bySteamBotRequestDto.AvatarImage != null)
                user.AvatarImage = bySteamBotRequestDto.AvatarImage;

            if (bySteamBotRequestDto.SteamProfileName != null)
                user.SteamProfileName = bySteamBotRequestDto.SteamProfileName;

            var result = await _userManager.CreateAsync(user, bySteamBotRequestDto.Password);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                string propertyName;
                switch (error)
                {
                    case Errors.InvalidIdCode:
                    case Errors.IdCodeReserved:
                    case Errors.IdCodeUsed:
                        propertyName = nameof(bySteamBotRequestDto.IdCode);
                        break;

                    case Errors.UserNameInvalidCharacter:
                    case Errors.UserNameInvalidLength:
                    case Errors.UserNameUsed:
                        propertyName = nameof(bySteamBotRequestDto.UserName);
                        break;

                    case Errors.InvalidEmail:
                        propertyName = nameof(bySteamBotRequestDto.Email);
                        break;

                    case Errors.InvalidSms:
                    case Errors.SmsUsed:
                        propertyName = nameof(bySteamBotRequestDto.SmsNumber);
                        break;

                    case Errors.AvatarImageUntrusted:
                        propertyName = nameof(bySteamBotRequestDto.AvatarImage);
                        break;

                    case Errors.PasswordAllWhitespace:
                    case Errors.PasswordTooShort:
                        propertyName = nameof(bySteamBotRequestDto.Password);
                        break;

                    default:
                        return this.BadRequest(nameof(bySteamBotRequestDto), error);
                }
                return this.BadRequest(nameof(bySteamBotRequestDto), propertyName, error);
            }

            await _userManager.AddLoginAsync(user.Id,
                new UserLoginInfo(KeylolLoginProviders.Steam, steamBindingToken.SteamId));
            _dbContext.SteamBindingTokens.Remove(steamBindingToken);
            await _dbContext.SaveChangesAsync();

            if (bySteamBotRequestDto.SteamCnUserName != null)
            {
                var steamCnUser =
                    await SteamCnProvider.UserLoginAsync(bySteamBotRequestDto.SteamCnUserName, bySteamBotRequestDto.SteamCnPassword, false);
                if (steamCnUser != null && steamCnUser.Uid > 0 &&
                    await _userManager.FindAsync(new UserLoginInfo(KeylolLoginProviders.SteamCn,
                        steamCnUser.Uid.ToString())) == null)
                {
                    await _userManager.AddLoginAsync(user.Id, new UserLoginInfo(KeylolLoginProviders.SteamCn,
                        steamCnUser.Uid.ToString()));
                    user.SteamCnUserName = steamCnUser.UserName;
                    user.SteamCnBindingTime = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                }
            }

            await _coupon.UpdateAsync(user, CouponEvent.新注册);

            var inviterText = string.Empty;
            if (bySteamBotRequestDto.InviterIdCode != null)
            {
                var inviter = await _userManager.FindByIdCodeAsync(bySteamBotRequestDto.InviterIdCode);
                if (inviter != null)
                {
                    user.InviterId = inviter.Id;
                    await _dbContext.SaveChangesAsync();
                    await _coupon.UpdateAsync(inviter, CouponEvent.邀请注册, new {UserId = user.Id});
                    await _coupon.UpdateAsync(user, CouponEvent.应邀注册, new {InviterId = user.Id});
                    inviterText = $"邀请人：{inviter.UserName} ({inviter.IdCode})\n";
                }
            }

            AutoSubscribe(user.Id);

            var operatorRoleId = (await _roleManager.FindByNameAsync(KeylolRoles.Operator)).Id;
            foreach (var @operator in await _dbContext.Users
                .Where(u => u.Roles.Any(r => r.RoleId == operatorRoleId)).ToListAsync())
            {
                await _userManager.SendSteamChatMessageAsync(@operator,
                    $"[新用户注册 {user.RegisterTime}]\n#{user.Sid} {user.UserName}\nSteam 昵称：{user.SteamProfileName}\nIP：{user.RegisterIp}\n{inviterText}https://www.keylol.com/user/{user.IdCode}");
            }

            return Ok(await _oneTimeToken.Generate(user.Id, TimeSpan.FromMinutes(1), OneTimeTokenPurpose.UserLogin));
        }

        private static void AutoSubscribe(string userId)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new KeylolDbContext())
                using (var userManager = new KeylolUserManager(dbContext))
                {
                    var redis = Global.Container.GetInstance<RedisProvider>();
                    var cachedData = new CachedDataProvider(dbContext, redis);
                    if (await SteamCrawlerProvider.UpdateUserSteamGameRecordsAsync(userId, dbContext, userManager,
                        redis, cachedData))
                    {
                        var games = await (from record in dbContext.UserSteamGameRecords
                            where record.UserId == userId
                            join point in dbContext.Points on record.SteamAppId equals point.SteamAppId
                            orderby record.TotalPlayedTime
                            select new
                            {
                                PointId = point.Id,
                                record.LastPlayTime
                            }).ToListAsync();

                        var gamePointIds = games.Select(g => g.PointId).ToList();
                        var mostPlayedPointIds = gamePointIds.Take(3).ToList();
                        var recentPlayedPointIds = games.Where(g => !mostPlayedPointIds.Contains(g.PointId))
                            .OrderByDescending(g => g.LastPlayTime)
                            .Select(g => g.PointId).Take(3).ToList();
                        var categoryPointIds = await (from relationship in dbContext.PointRelationships
                            where gamePointIds.Contains(relationship.SourcePointId) &&
                                  (relationship.Relationship == PointRelationshipType.Tag ||
                                   relationship.Relationship == PointRelationshipType.Series)
                            group 1 by relationship.TargetPointId
                            into g
                            orderby g.Count() descending
                            select g.Key).Take(3).ToListAsync();

                        var pointIds = mostPlayedPointIds.Concat(recentPlayedPointIds).Concat(categoryPointIds).ToList();
                        foreach (var pointId in pointIds)
                        {
                            await cachedData.Subscriptions.AddAsync(userId, pointId, SubscriptionTargetType.Point);
                        }

                        var pointFeedStreams = pointIds.Select(PointStream.Name).ToList();
                        var feeds = await (from feed in dbContext.Feeds
                            where pointFeedStreams.Contains(feed.StreamName)
                            orderby feed.Id descending
                            group new {feed.Id, feed.StreamName} by new {feed.Entry, feed.EntryType}
                            into g
                            orderby g.Max(f => f.Id)
                            select g).Take(120).ToListAsync();
                        var subscriptionStream = SubscriptionStream.Name(userId);
                        foreach (var feed in feeds)
                        {
                            var properties = new SubscriptionStream.FeedProperties
                            {
                                Reasons = feed.Select(f => f.StreamName.Split(':')[1]).Distinct()
                                    .Select(id => $"point:{id}").ToList()
                            };
                            dbContext.Feeds.Add(new Models.Feed
                            {
                                StreamName = subscriptionStream,
                                Entry = feed.Key.Entry,
                                EntryType = feed.Key.EntryType,
                                Properties = JsonConvert.SerializeObject(properties)
                            });
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            });
        }


        /// <summary>
        ///     请求 DTO
        /// </summary>
        public class UserCreateOneBySteamBotRequestDto
        {
            /// <summary>
            ///     识别码
            /// </summary>
            [Utilities.Required]
            public string IdCode { get; set; }

            /// <summary>
            ///     昵称（用户名）
            /// </summary>
            [Utilities.Required]
            public string UserName { get; set; }

            /// <summary>
            ///     口令
            /// </summary>
            [Utilities.Required]
            public string Password { get; set; }

            /// <summary>
            ///     头像
            /// </summary>
            public string AvatarImage { get; set; }

            /// <summary>
            /// 邮箱
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// 手机
            /// </summary>
            public string SmsNumber { get; set; }

            /// <summary>
            /// SteamCN 用户名
            /// </summary>
            public string SteamCnUserName { get; set; }

            /// <summary>
            /// SteamCN 密码
            /// </summary>
            public string SteamCnPassword { get; set; }

            /// <summary>
            ///     SteamBindingToken Id
            /// </summary>
            [Utilities.Required]
            public string SteamBindingTokenId { get; set; }

            /// <summary>
            ///     Steam 玩家昵称
            /// </summary>
            public string SteamProfileName { get; set; }

            /// <summary>
            ///     邀请人识别码
            /// </summary>
            public string InviterIdCode { get; set; }
        }
    }
}