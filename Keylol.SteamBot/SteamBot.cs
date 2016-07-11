using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using ChannelAdam.ServiceModel;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;
using SteamKit2.Discovery;
using SteamKit2.Internal;

namespace Keylol.SteamBot
{
    public class SteamBot : KeylolService
    {
        private readonly Timer _heartbeatTimer = new Timer(10000) {AutoReset = false}; // 10s
        private readonly ILog _logger;

        public SteamBot(ILogProvider logProvider, IServiceConsumer<ISteamBotCoordinator> coordinator,
            SteamBotCoordinatorCallback callback)
        {
            ServiceName = "Keylol.SteamBot";

            _logger = logProvider.Logger;
            Coordinator = coordinator;
            callback.SteamBot = this;

            _heartbeatTimer.Elapsed += (sender, args) =>
            {
                try
                {
                    Coordinator.Operations.Ping();
                }
                catch (Exception e)
                {
                    _logger.Warn("Ping failed.", e);
                    Coordinator.Close();
                }
                _heartbeatTimer.Start();
            };
        }

        public IServiceConsumer<ISteamBotCoordinator> Coordinator { get; }

        public List<BotInstance> BotInstances { get; set; }

        protected override void OnStart(string[] args)
        {
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            Directory.CreateDirectory(dataFolder);
            _logger.Info($"Application data folder: {dataFolder}");

            uint cellId = 0; // 基于地理位置
            if (args != null && args.Length > 1) uint.TryParse(args[0], out cellId);
            CMClient.Servers.CellID = cellId;
            CMClient.Servers.ServerListProvider = new FileStorageServerListProvider(Path.Combine(dataFolder, "server-list.bin"));

            try
            {
                Coordinator.Operations.RequestBots();
                _heartbeatTimer.Start();
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to bootstrap.", e);
                throw;
            }
        }

        protected override void OnStop()
        {
            _heartbeatTimer.Stop();
            if (BotInstances != null)
                foreach (var botInstance in BotInstances)
                {
                    botInstance.Dispose();
                }
            base.OnStop();
        }
    }
}