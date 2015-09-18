﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using DevTrends.WCFDataAnnotations;
using Keylol.DAL;
using Keylol.Hubs;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Services.Contracts;
using Microsoft.AspNet.SignalR;

namespace Keylol.Services
{
    [ValidateDataAnnotationsBehavior]
    public class SteamBotCoodinator : ISteamBotCoodinator
    {
        private bool _botAllocated;
        private readonly KeylolDbContext _dbContext = new KeylolDbContext();
        private readonly SteamBotManager _client;

        public static ConcurrentDictionary<string, ISteamBotCoodinatorCallback> Clients =
            new ConcurrentDictionary<string, ISteamBotCoodinatorCallback>();

        public SteamBotCoodinator()
        {
            OperationContext.Current.Channel.Closed += OnClientClosed;
            OperationContext.Current.Channel.Faulted += OnClientClosed;
            _client =
                _dbContext.SteamBotManagers.Single(
                    manager => manager.ClientId == OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name);
            Clients[_client.Id] = OperationContext.Current.GetCallbackChannel<ISteamBotCoodinatorCallback>();
        }

        [OperationContract]
        public async Task<IEnumerable<SteamBotDTO>> AllocateBots()
        {
            if (_botAllocated)
            {
                throw new FaultException("Only one allocation can be performed per session.");
            }
            _botAllocated = true;

            var bots = await _dbContext.SteamBots.Where(bot => bot.Manager == null).ToListAsync();
            foreach (var bot in bots)
            {
                bot.Online = false;
                bot.Manager = _client;
            }
            await _dbContext.SaveChangesAsync();
            return bots.Select(bot => new SteamBotDTO(bot));
        }

        [OperationContract]
        public async Task UpdateBots(IEnumerable<SteamBotVM> vms)
        {
            foreach (var vm in vms)
            {
                var bot = await _dbContext.SteamBots.FindAsync(vm.Id);
                if (vm.SteamId != null)
                    bot.SteamId = vm.SteamId;
                if (vm.FriendCount != null)
                    bot.FriendCount = vm.FriendCount.Value;
                if (vm.Online != null)
                    bot.Online = vm.Online.Value;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserDTO> GetUserBySteamId(long steamId)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.SteamId == steamId);
            return user == null ? null : new UserDTO(user) {SteamBot = new SteamBotDTO(user.SteamBot)};
        }

        public async Task<bool> BindSteamUserWithBindingToken(long userSteamId, string code, string botId)
        {
            var token =
                await _dbContext.SteamBindingTokens.SingleOrDefaultAsync(t => t.Code == code && t.Bot.Id == botId);
            if (token == null)
                return false;

            token.SteamId = userSteamId;
            await _dbContext.SaveChangesAsync();
            GlobalHost.ConnectionManager.GetHubContext<SteamBindingHub, ISteamBindingHubClient>()
                .Clients.Client(token.BrowserConnectionId)?
                .NotifyCodeReceived();
            return true;
        }

        public async Task<bool> BindSteamUserWithLoginToken(long userSteamId, string code)
        {
            var token =
                await _dbContext.SteamLoginTokens.SingleOrDefaultAsync(t => t.Code == code);
            if (token == null)
                return false;

            token.SteamId = userSteamId;
            await _dbContext.SaveChangesAsync();
            GlobalHost.ConnectionManager.GetHubContext<SteamLoginHub, ISteamLoginHubClient>()
                .Clients.Client(token.BrowserConnectionId)?
                .NotifyCodeReceived();
            return true;
        }

        private async void OnClientClosed(object sender, EventArgs eventArgs)
        {
            ISteamBotCoodinatorCallback callback;
            Clients.TryRemove(_client.Id, out callback);
            _client.Bots.Clear();
            await _dbContext.SaveChangesAsync();
            _dbContext.Dispose();
        }
    }
}