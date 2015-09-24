using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.DAL;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.SignalR;

namespace Keylol.Hubs
{
    public interface ISteamLoginHubClient
    {
        void NotifyCodeReceived();
    }

    public class SteamLoginHub : Hub<ISteamLoginHubClient>
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
            var tokens = await _dbContext.SteamLoginTokens.Where(t => t.BrowserConnectionId == Context.ConnectionId).ToListAsync();
            foreach (var token in tokens)
            {
                _dbContext.SteamLoginTokens.Remove(token);
            }
            await _dbContext.SaveChangesAsync();
            await base.OnDisconnected(stopCalled);
        }

        public async Task<SteamLoginTokenDTO> CreateToken()
        {
            var token = new SteamLoginToken()
            {
                Code = await SteamLoginToken.GenerateCodeAsync(_dbContext),
                BrowserConnectionId = Context.ConnectionId
            };
            _dbContext.SteamLoginTokens.Add(token);
            await _dbContext.SaveChangesAsync();
            return new SteamLoginTokenDTO(token);
        }
    }
}