using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;

namespace Keylol.SteamBot
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SteamBotCoordinatorCallback : ISteamBotCoordinatorCallback
    {
        private readonly ILog _logger;

        public SteamBotCoordinatorCallback(ILogProvider logProvider)
        {
            _logger = logProvider.Logger;
        }

        public SteamBot SteamBot { get; set; }

        public string[] GetAllocatedBots()
        {
            return SteamBot.BotInstances?.Select(b => b.Id).ToArray();
        }

        public void RequestReallocateBots(int count)
        {
            _logger.Info($"Received new allocation target: {count} bot(s).");
            if (SteamBot.BotInstances == null) SteamBot.BotInstances = new List<BotInstance>(count);
            if (SteamBot.BotInstances.Count < count)
            {
                var botInfos = SteamBot.Coordinator.Operations.AllocateBots(count - SteamBot.BotInstances.Count);
                var newBots = botInfos.Select(b =>
                {
                    var botInstance = Program.Container.GetInstance<BotInstance>();
                    botInstance.Id = b.Id;
                    Debug.Assert(b.SequenceNumber != null);
                    botInstance.SequenceNumber = b.SequenceNumber.Value;
                    botInstance.LogOnDetails.Username = b.SteamUserName;
                    botInstance.LogOnDetails.Password = b.SteamPassword;
                    return botInstance;
                }).ToList();
                SteamBot.BotInstances.AddRange(newBots);
                foreach (var botInstance in newBots)
                {
                    botInstance.Start();
                }
            }
            else if (SteamBot.BotInstances.Count > count)
            {
                var deallocCount = SteamBot.BotInstances.Count - count;
                var botToDealloc = SteamBot.BotInstances.Take(deallocCount).ToList();
                SteamBot.BotInstances.RemoveRange(0, deallocCount);
                foreach (var botInstance in botToDealloc)
                {
                    botInstance.Dispose();
                }
            }
        }

        public void RemoveSteamFriend(string botId, string steamId)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string botId, string steamId, string message)
        {
            throw new NotImplementedException();
        }

        public string FetchUrl(string botId, string url)
        {
            throw new NotImplementedException();
        }
    }
}