using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Timers;
using ChannelAdam.ServiceModel;
using CsQuery;
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
        private readonly Timer _medalRefreshTimer = new Timer(10*60*1000); // 10 min
        private readonly ILog _logger;

        public static ChinaMedal Medal { get; set; } = new ChinaMedal();

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

            _medalRefreshTimer.Elapsed += OnMedalRefreshTimerElapsed;
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
            CMClient.Servers.ServerListProvider =
                new FileStorageServerListProvider(Path.Combine(dataFolder, "server-list.bin"));

            try
            {
                OnMedalRefreshTimerElapsed(this, null);
                Coordinator.Operations.RequestBots();
                _heartbeatTimer.Start();
                _medalRefreshTimer.Start();
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
            _medalRefreshTimer.Stop();
            if (BotInstances != null)
                foreach (var botInstance in BotInstances)
                {
                    botInstance.Dispose();
                }
            base.OnStop();
        }

        private async void OnMedalRefreshTimerElapsed(object sender, ElapsedEventArgs args)
        {
            var request = WebRequest.CreateHttp("https://www.rio2016.com/en/medal-count-country");
            request.Timeout = 30000;
            request.Referer = "https://www.rio2016.com/en/medal-count";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml, application/json;q=0.9,*/*;q=0.8";
            request.Headers["Accept-Language"] = "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4";
            request.Timeout = 30000; // 30s
            request.ReadWriteTimeout = 100000; // 100s
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            using (var response = await request.GetResponseAsync())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    return;
                var dom = CQ.Create(responseStream);
                var tr = dom["tr[data-odfcode=\"CHN\"]"];
                Medal.Rank = int.Parse(tr.Find(".col-1").Text());
                Medal.Gold = int.Parse(tr.Find(".col-4").Text());
                Medal.Silver = int.Parse(tr.Find(".col-5").Text());
                Medal.Bronze = int.Parse(tr.Find(".col-6").Text());
            }

            if (BotInstances == null || args == null)
                return;
            foreach (var bot in BotInstances)
            {
                bot.UpdatePlayingGame();
            }
        }

        public class ChinaMedal
        {
            public int Gold { get; set; }

            public int Silver { get; set; }

            public int Bronze { get; set; }

            public int Rank { get; set; }
        }
    }
}