using System;
using System.Collections.Generic;
using System.IO;
using ChannelAdam.ServiceModel;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SteamKit2;

namespace Keylol.SteamBot
{
    public class SteamBot : KeylolService
    {
        private readonly ILog _logger;
        private readonly RetryPolicy _retryPolicy;

        public IServiceConsumer<ISteamBotCoordinator> Coordinator { get; }

        public List<BotInstance> BotInstances { get; set; }

        public SteamBot(ILogProvider logProvider, IServiceConsumer<ISteamBotCoordinator> coordinator,
            SteamBotCoordinatorCallback callback, RetryPolicy retryPolicy)
        {
            ServiceName = "Keylol.SteamBot";

            _logger = logProvider.Logger;
            Coordinator = coordinator;
            _retryPolicy = retryPolicy;
            callback.SteamBot = this;
        }

        protected override async void OnStart(string[] args)
        {
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            Directory.CreateDirectory(dataFolder);
            _logger.Info($"Application data folder: {dataFolder}");

            uint cellId = 0; // 基于地理位置
            if (args != null && args.Length > 1) uint.TryParse(args[0], out cellId);
            _logger.Info("Loading CM server list...");
            await _retryPolicy.ExecuteAsync(async () => await SteamDirectory.Initialize(cellId));
            _logger.Info("CM server list loaded.");

            try
            {
                Coordinator.Operations.RequestBots();
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to bootstrap.", e);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (BotInstances != null)
                foreach (var botInstance in BotInstances)
                {
                    botInstance.Dispose();
                }
            base.OnStop();
        }
    }
}