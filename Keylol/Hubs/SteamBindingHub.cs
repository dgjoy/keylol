using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services;
using Keylol.Services.Contracts;
using Microsoft.AspNet.SignalR;
using SteamKit2;

namespace Keylol.Hubs
{
    public interface ISteamBindingHubClient
    {
        void NotifySteamFriendAdded();
        void NotifyCodeReceived(string steamProfileName, string steamAvatarHash);
    }

    public class SteamBindingHub : Hub<ISteamBindingHubClient>
    {
        private readonly KeylolDbContext _dbContext = new KeylolDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var tokens =
                await
                    _dbContext.SteamBindingTokens.Where(t => t.BrowserConnectionId == Context.ConnectionId)
                        .ToListAsync();
            foreach (var token in tokens)
            {
                if (token.SteamId != null)
                {
                    SteamBotCoordinator botCoordinator;
                    if (SteamBotCoordinator.Sessions.TryGetValue(token.Bot.SessionId, out botCoordinator))
                        botCoordinator.Client.RemoveSteamFriend(token.BotId, token.SteamId);
                }
                _dbContext.SteamBindingTokens.Remove(token);
            }
            await _dbContext.SaveChangesAsync();
            await base.OnDisconnected(stopCalled);
        }

        public async Task<SteamBindingTokenDto> CreateToken()
        {
            var bots =
                await
                    _dbContext.SteamBots.Where(b =>
                        b.Online && b.SessionId != null && b.SteamId != null && b.FriendCount < b.FriendUpperLimit &&
                        b.Enabled).ToListAsync();
            var random = new Random();
            var bot = bots.Skip(random.Next(0, bots.Count)).FirstOrDefault();
            if (bot == null)
                return null;

            string code;
            do
            {
                var sb = new StringBuilder();
                for (var i = 0; i < 4; i++)
                {
                    sb.Append((char) random.Next('A', 'Z'));
                }
                for (var i = 0; i < 4; i++)
                {
                    sb.Append(random.Next(0, 10));
                }
                code = sb.ToString();
            } while (await _dbContext.SteamBindingTokens.AnyAsync(t => t.Code == code));

            var token = new SteamBindingToken
            {
                Code = code,
                Bot = bot,
                BrowserConnectionId = Context.ConnectionId
            };
            _dbContext.SteamBindingTokens.Add(token);
            await _dbContext.SaveChangesAsync();

            var steamId = new SteamID();
            steamId.SetFromSteam3String(token.Bot.SteamId);
            return new SteamBindingTokenDto
            {
                Id = token.Id,
                Code = token.Code,
                BotSteamId64 = steamId.ConvertToUInt64().ToString()
            };
        }
    }
}