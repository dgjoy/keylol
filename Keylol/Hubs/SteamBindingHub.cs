using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.DAL;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    public interface ISteamBindingHubClient
    {
        void NotifySteamFriendAdded();
        void NotifyCodeReceived();
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
            var token = await _dbContext.SteamBindingTokens.SingleOrDefaultAsync(
                t => t.BrowserConnectionId == Context.ConnectionId);
            if (token != null)
            {
                _dbContext.SteamBindingTokens.Remove(token);
                await _dbContext.SaveChangesAsync();
            }
            await base.OnDisconnected(stopCalled);
        }

        public async Task<SteamBindingTokenDTO> CreateToken()
        {
            var token = new SteamBindingToken
            {
                Code = await SteamBindingToken.GenerateCodeAsync(_dbContext),
                BrowserConnectionId = Context.ConnectionId
            };
            _dbContext.SteamBindingTokens.Add(token);
            await _dbContext.SaveChangesAsync();
            return new SteamBindingTokenDTO(token);
        }
    }
}