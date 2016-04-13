using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;
using DevTrends.WCFDataAnnotations;
using Keylol.Hubs;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.Services.Contracts;
using Keylol.Utilities;
using Microsoft.AspNet.SignalR;
using StatusClaim = Keylol.Services.Contracts.StatusClaim;

namespace Keylol.Services
{
    /// <summary>
    /// <see cref="ISteamBotCoordinator"/> 实现
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SteamBotCoordinator : ISteamBotCoordinator
    {
        /// <summary>
        /// 所有正在进行的会话，Key 为 Session ID，Value 为 <see cref="SteamBotCoordinator"/>
        /// </summary>
        public static ConcurrentDictionary<string, SteamBotCoordinator> Sessions { get; } =
            new ConcurrentDictionary<string, SteamBotCoordinator>();

        /// <summary>
        /// 当前会话 ID
        /// </summary>
        public string SessionId { get; } = OperationContext.Current.SessionId;

        /// <summary>
        /// 当前会话的客户端
        /// </summary>
        public ISteamBotCoordinatorCallback Client { get; } =
            OperationContext.Current.GetCallbackChannel<ISteamBotCoordinatorCallback>();

        /// <summary>
        /// 创建 <see cref="SteamBotCoordinator"/>
        /// </summary>
        public SteamBotCoordinator()
        {
            Sessions[SessionId] = this;
            OperationContext.Current.InstanceContext.Closing += OnSessionEnd;
            OnSessionBegin();
        }

        private async void OnSessionBegin()
        {
            // 更新已分配机器人的 SessionId
            try
            {
                var allocatedBots = await Client.GetAllocatedBots();
                if (allocatedBots != null)
                {
                    using (var dbContext = new KeylolDbContext())
                    {
                        var bots = await dbContext.SteamBots.Where(b => allocatedBots.Contains(b.Id)).ToListAsync();
                        foreach (var bot in bots)
                        {
                            bot.SessionId = SessionId;
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                OperationContext.Current.Channel.Close();
            }
        }

        private async void OnSessionEnd(object sender, EventArgs eventArgs)
        {
            try
            {
                SteamBotCoordinator coordinator;
                Sessions.TryRemove(SessionId, out coordinator);
                if (Sessions.Count > 0)
                    await ReallocateBots(Sessions.Values.Last());
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// 重新计算每个客户端应该分配的机器人数量并通知客户端
        /// </summary>
        /// <param name="newSession">新 Session，在分配机器人时会最后分配</param>
        private static async Task ReallocateBots(SteamBotCoordinator newSession)
        {
            try
            {
                using (var dbContext = new KeylolDbContext())
                {
                    var botCount = await dbContext.SteamBots.CountAsync(b => b.Enabled);
                    var averageCount = botCount/Sessions.Count;
                    var lastCount = botCount - averageCount*(Sessions.Count - 1);
                    foreach (var session in Sessions.Values.Where(s => s != newSession))
                    {
                        await session.Client.RequestReallocateBots(averageCount);
                    }
                    await newSession.Client.RequestReallocateBots(lastCount);
                }
            }
            catch (Exception)
            {
                OperationContext.Current.Channel.Close();
            }
        }

        /// <summary>
        /// 请求分配机器人，协作器在计算好机器人数量后将通过 RequestReallocateBots 回调通知客户端
        /// </summary>
        public async Task RequestBots()
        {
            await ReallocateBots(this);
        }

        /// <summary>
        /// 请求分配指定数量的机器人
        /// </summary>
        /// <param name="count">要求分配的数量</param>
        /// <returns>分配给客户端的机器人列表</returns>
        public async Task<List<SteamBotDto>> AllocateBots(int count)
        {
            using (var dbContext = new KeylolDbContext())
            {
                // 机器人的 SessionId 不存在 Sessions 中则认定为没有被分配过
                var bots = await dbContext.SteamBots.Where(bot => !Sessions.Keys.Contains(bot.SessionId) && bot.Enabled)
                    .Take(() => count)
                    .ToListAsync();
                foreach (var bot in bots)
                {
                    bot.Online = false;
                    bot.SessionId = SessionId;
                }
                await dbContext.SaveChangesAsync();
                return bots.Select(bot => new SteamBotDto(bot, true)).ToList();
            }
        }

        /// <summary>
        /// 撤销对指定机器人的会话分配
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        public async Task DeallocateBot(string botId)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var bot = await dbContext.SteamBots.FindAsync(botId);
                bot.Online = false;
                bot.SessionId = null;
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 更新指定用户的属性
        /// </summary>
        /// <param name="steamId">要更新的用户 Steam ID</param>
        /// <param name="profileName">Steam 昵称，<c>null</c> 表示不更新</param>
        public async Task UpdateUser(string steamId, string profileName)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var user = await dbContext.Users.SingleOrDefaultAsync(u => u.SteamId == steamId);
                if (user == null)
                    return;
                if (profileName != null)
                    user.SteamProfileName = profileName;
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 更新指定机器人的属性
        /// </summary>
        /// <param name="id">要更新的机器人 ID</param>
        /// <param name="friendCount">好友数，<c>null</c> 表示不更新</param>
        /// <param name="online">是否在线，<c>null</c> 表示不更新</param>
        /// <param name="steamId">Steam ID，<c>null</c> 表示不更新</param>
        public async Task UpdateBot(string id, int? friendCount = null, bool? online = null, string steamId = null)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var bot = await dbContext.SteamBots.FindAsync(id);
                if (bot == null)
                    return;
                if (friendCount != null)
                    bot.FriendCount = friendCount.Value;
                if (online != null)
                    bot.Online = online.Value;
                if (steamId != null)
                    bot.SteamId = steamId;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<UserDto> GetUserBySteamId(string steamId)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var user =
                    await dbContext.Users.Include(u => u.SteamBot).SingleOrDefaultAsync(u => u.SteamId == steamId);
                return user == null ? null : new UserDto(user, true, true) {SteamBot = new SteamBotDto(user.SteamBot)};
            }
        }

        public async Task<IList<UserDto>> GetUsersBySteamIds(IList<string> steamIds)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var users =
                    await
                        dbContext.Users.Include(u => u.SteamBot).Where(u => steamIds.Contains(u.SteamId)).ToListAsync();
                return
                    users.Select(user => new UserDto(user, true, true) {SteamBot = new SteamBotDto(user.SteamBot)})
                        .ToList();
            }
        }

        public async Task SetUserStatus(string steamId, StatusClaim status)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var user = await dbContext.Users.SingleOrDefaultAsync(u => u.SteamId == steamId);
                if (user != null)
                {
                    var userManager = KeylolUserManager.Create(dbContext);
                    switch (status)
                    {
                        case StatusClaim.Normal:
                            await userManager.RemoveStatusClaimAsync(user.Id);
                            break;

                        case StatusClaim.Probationer:
                            await userManager.SetStatusClaimAsync(user.Id, Utilities.StatusClaim.Probationer);
                            break;
                    }
                }
            }
        }

        public async Task DeleteBindingToken(string botId, string steamId)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var tokens =
                    await
                        dbContext.SteamBindingTokens.Where(t => t.SteamId == steamId && t.BotId == botId).ToListAsync();
                dbContext.SteamBindingTokens.RemoveRange(tokens);
                await dbContext.SaveChangesAsync();
            }
        }

        public Task<string> GetCMServer()
        {
            return Task.FromResult("58.215.54.121:27018");
        }

        public async Task<bool> BindSteamUserWithBindingToken(string code, string botId, string userSteamId,
            string userSteamProfileName, string userSteamAvatarHash)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var token =
                    await
                        dbContext.SteamBindingTokens.SingleOrDefaultAsync(
                            t => t.Code == code && t.BotId == botId && t.SteamId == null);
                if (token == null)
                    return false;

                token.SteamId = userSteamId;
                await dbContext.SaveChangesAsync();
                GlobalHost.ConnectionManager.GetHubContext<SteamBindingHub, ISteamBindingHubClient>()
                    .Clients.Client(token.BrowserConnectionId)?
                    .NotifyCodeReceived(userSteamProfileName, userSteamAvatarHash);
                return true;
            }
        }

        public async Task<bool> BindSteamUserWithLoginToken(string userSteamId, string code)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var token =
                    await dbContext.SteamLoginTokens.SingleOrDefaultAsync(t => t.Code == code);
                if (token == null)
                    return false;

                token.SteamId = userSteamId;
                await dbContext.SaveChangesAsync();
                GlobalHost.ConnectionManager.GetHubContext<SteamLoginHub, ISteamLoginHubClient>()
                    .Clients.Client(token.BrowserConnectionId)?
                    .NotifyCodeReceived();
                return true;
            }
        }

        public async Task BroadcastBotOnFriendAdded(string botId)
        {
            using (var dbContext = new KeylolDbContext())
            {
                GlobalHost.ConnectionManager.GetHubContext<SteamBindingHub, ISteamBindingHubClient>()
                    .Clients.Clients(
                        await dbContext.SteamBots.Where(bot => bot.Id == botId)
                            .SelectMany(bot => bot.BindingTokens)
                            .Select(token => token.BrowserConnectionId)
                            .ToListAsync()
                    )?
                    .NotifySteamFriendAdded();
            }
        }
    }
}