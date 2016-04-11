using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Keylol.SteamBot.ServiceReference;
using SteamKit2;
using SteamKit2.Internal;
using Timer = System.Timers.Timer;

namespace Keylol.SteamBot
{
    public partial class SteamBotService : ServiceBase
    {
        public string AppDataFolder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Keylol Steam Bot Service");

        public bool IsRunning { get; private set; }

        public SteamBotCoordinatorClient Coordinator { get; private set; }

        public static object ConsoleOutputLock { get; } = new object();

        public static object ConsoleInputLock { get; } = new object();

        private readonly Dictionary<string, Bot> _bots = new Dictionary<string, Bot>();
        private readonly Timer _healthReportTimer = new Timer(60*1000); // 60s

        public const uint GlobalMaxRetryCount = 3;
        public const int GlobalRetryCooldownDuration = 2000;

        public SteamBotService()
        {
            InitializeComponent();
            _healthReportTimer.Elapsed += (sender, args) => { ReportBotHealthAsync().Wait(); };
        }

        private SteamBotCoordinatorClient CreateProxy()
        {
            var proxy = new SteamBotCoordinatorClient(new InstanceContext(new SteamBotCoordinatorCallbackHandler(this)));
            if (proxy.ClientCredentials != null)
            {
                proxy.ClientCredentials.UserName.UserName = "keylol-bot";
                proxy.ClientCredentials.UserName.Password = "neLFDyJB8Vj2Xtsn2KMTUEFw";
            }
            proxy.InnerChannel.Faulted += (sender, a) =>
            {
                WriteLog("Communication channel faulted. Recreating...", EventLogEntryType.Error);
                OnStop();
                Thread.Sleep(GlobalRetryCooldownDuration);
                OnStart(null);
            };
            return proxy;
        }

        public void WriteLog(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            if (Environment.UserInteractive)
            {
                lock (ConsoleOutputLock)
                {
                    switch (type)
                    {
                        case EventLogEntryType.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;

                        case EventLogEntryType.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case EventLogEntryType.Information:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;

                        case EventLogEntryType.SuccessAudit:
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;

                        case EventLogEntryType.FailureAudit:
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            break;
                    }
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
            }
            else
            {
                _eventLog.WriteEntry(message, type);
            }
        }

        protected override async void OnStart(string[] args)
        {
            try
            {
                Directory.CreateDirectory(AppDataFolder);
                WriteLog($"Application data location: {AppDataFolder}");
                Coordinator = CreateProxy();
                WriteLog("Channel created.");
                WriteLog($"Coordinator endpoint: {Coordinator.Endpoint.Address}");
                var bots = await Coordinator.AllocateBotsAsync();
                WriteLog($"{bots.Length} {(bots.Length > 1 ? "bots" : "bot")} allocated.");
                foreach (var bot in bots)
                {
                    _bots[bot.Id] = new Bot(this, bot);
                }
                CMClient.Servers.Clear();
                uint cellId = 46; // Beijing
                if (args != null && args.Length > 1) uint.TryParse(args[0], out cellId);
                CMClient.Servers.TryAddRange((await Utils.Retry(async () => await SteamDirectory.LoadAsync(cellId), i =>
                {
                    if (i == 1)
                        WriteLog("Loading CM server list...");
                    else
                        WriteLog($"Loading CM server list... [{i}]", EventLogEntryType.Warning);
                }, uint.MaxValue)).Take(15));
                WriteLog("CM server list loaded.");
                IsRunning = true;
                foreach (var bot in _bots.Values)
                {
                    bot.Start();
                }
                _healthReportTimer.Start();
            }
            catch (CommunicationException e)
            {
                WriteLog($"Communication exception occurred: {e.Message}", EventLogEntryType.Warning);
            }
        }

        protected override void OnStop()
        {
            IsRunning = false;
            _healthReportTimer.Stop();

            if (Coordinator.State == CommunicationState.Faulted)
                Coordinator.Abort();
            else
                Coordinator.Close();

            WriteLog("Channel destroyed.");

            foreach (var bot in _bots.Values)
            {
                bot.Dispose();
            }
            _bots.Clear();
        }

        private async Task ReportBotHealthAsync()
        {
            await Coordinator.UpdateBotsAsync(_bots.Values.Select(bot =>
            {
                var online = bot.State == Bot.BotState.LoggedOnOnline;
                var vm = new SteamBotUpdateRequestDto
                {
                    Id = bot.Id,
                    Online = online
                };
                if (online)
                {
                    vm.FriendCount = bot.FriendCount;
                    vm.SteamId = bot.SteamId;
                }
                return vm;
            }).ToArray());
        }

        public void ConsoleStartup(string[] args)
        {
            Console.WriteLine("Running in console mode. Press Ctrl-M to stop.");
            OnStart(args);
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.M)
                    break;
            }
            OnStop();
        }

        private class SteamBotCoordinatorCallbackHandler : ISteamBotCoordinatorCallback
        {
            private readonly SteamBotService _botService;

            public SteamBotCoordinatorCallbackHandler(SteamBotService botService)
            {
                _botService = botService;
            }

            public void RemoveSteamFriend(string botId, string steamId)
            {
                Bot bot;
                if (!_botService._bots.TryGetValue(botId, out bot)) return;
                if (bot.State == Bot.BotState.LoggedOnOnline)
                    bot.RemoveFriend(steamId);
            }

            public void SendMessage(string botId, string steamId, string message)
            {
                Bot bot;
                if (!_botService._bots.TryGetValue(botId, out bot)) return;
                if (bot.State == Bot.BotState.LoggedOnOnline)
                    bot.SendMessage(steamId, message);
            }

            public string FetchUrl(string botId, string url)
            {
                Bot bot;
                if (!_botService._bots.TryGetValue(botId, out bot)) return null;
                var response = bot.Crawler.RequestAsync(url, "GET").Result.GetResponseStream();
                if (response == null) return null;
                using (var reader = new StreamReader(response))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}