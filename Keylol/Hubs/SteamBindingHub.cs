using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.DAL;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Services;
using Keylol.Services.Contracts;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    public interface ISteamBindingHubClient
    {
        void NotifySteamFriendAdded();
        void NotifyCodeReceived(string tokenId);
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
            var tokens = await _dbContext.SteamBindingTokens.Where(t => t.BrowserConnectionId == Context.ConnectionId).ToListAsync();
            foreach (var token in tokens)
            {
                if (token.SteamId != null && !token.Consumed)
                {
                    ISteamBotCoodinatorCallback callback;
                    if (SteamBotCoodinator.Clients.TryGetValue(token.Bot.SessionId, out callback))
                        callback.RemoveSteamFriend(token.Bot.Id, token.SteamId);
                }
                _dbContext.SteamBindingTokens.Remove(token);
            }
            await _dbContext.SaveChangesAsync();
            await base.OnDisconnected(stopCalled);
        }

        public async Task<SteamBindingTokenDTO> CreateToken()
        {
            var bot =
                await
                    _dbContext.SteamBots.FirstOrDefaultAsync(
                        b => b.Online && b.SessionId != null && b.FriendCount < b.FriendUpperLimit);
            if (bot == null)
                return null;

            var token = new SteamBindingToken
            {
                Code = await SteamBindingToken.GenerateCodeAsync(_dbContext),
                Bot = bot,
                BrowserConnectionId = Context.ConnectionId
            };
            _dbContext.SteamBindingTokens.Add(token);
            await _dbContext.SaveChangesAsync();
            return new SteamBindingTokenDTO(token);
        }
    }
}