using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
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
            var tokens =
                await
                    _dbContext.SteamLoginTokens.Where(t => t.BrowserConnectionId == Context.ConnectionId).ToListAsync();
            foreach (var token in tokens)
            {
                _dbContext.SteamLoginTokens.Remove(token);
            }
            await _dbContext.SaveChangesAsync();
            await base.OnDisconnected(stopCalled);
        }

        public async Task<SteamLoginTokenDto> CreateToken()
        {
            string code;
            var random = new Random();
            do
            {
                code = random.Next(0, 10000).ToString("D4");
            } while (await _dbContext.SteamLoginTokens.AnyAsync(t => t.Code == code));
            var token = new SteamLoginToken
            {
                Code = code,
                BrowserConnectionId = Context.ConnectionId
            };
            _dbContext.SteamLoginTokens.Add(token);
            await _dbContext.SaveChangesAsync();
            return new SteamLoginTokenDto
            {
                Id = token.Id,
                Code = token.Code
            };
        }
    }
}