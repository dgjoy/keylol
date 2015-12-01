using System;
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

        public SteamBotCoodinatorClient Coodinator { get; private set; }

        public static object ConsoleOutputLock { get; } = new object();

        public static object ConsoleInputLock { get; } = new object();

        public static uint GlobalMaxRetryCount { get; set; } = 3;

        private Bot[] _bots;
        private readonly Timer _healthReportTimer = new Timer(60*1000); // 60s

        public SteamBotService()
        {
            InitializeComponent();
            _healthReportTimer.Elapsed += async (sender, args) =>
            {
                try
                {
                    await ReportBotHealthAsync();
                }
                catch (Exception)
                {
                    // ignore
                }
            };
        }

        private SteamBotCoodinatorClient CreateProxy()
        {
            var proxy = new SteamBotCoodinatorClient(new InstanceContext(new SteamBotCoodinatorCallbackHandler(this)));
            if (proxy.ClientCredentials != null)
            {
                proxy.ClientCredentials.UserName.UserName = "keylol-bot";
                proxy.ClientCredentials.UserName.Password = "neLFDyJB8Vj2Xtsn2KMTUEFw";
            }
            proxy.InnerChannel.Faulted += (sender, a) =>
            {
                WriteLog("Communication channel faulted. Recreating...", EventLogEntryType.Error);
                OnStop();
                Thread.Sleep(3000);
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
                Coodinator = CreateProxy();
                WriteLog("Channel created.");
                WriteLog($"Coodinator endpoint: {Coodinator.Endpoint.Address}");
//                var cmServer = await Coodinator.GetCMServerAsync();
//                var parts = cmServer.Split(':');
//                _cmServer = new IPEndPoint(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
//                WriteLog($"CM Server: {_cmServer}");
                var bots = await Coodinator.AllocateBotsAsync();
                WriteLog($"{bots.Length} {(bots.Length > 1 ? "bots" : "bot")} allocated.");
                _bots = bots.Select(bot => new Bot(this, bot)).ToArray();
                CMClient.Servers.Clear();
                CMClient.Servers.TryAddRange((await SteamDirectory.LoadAsync(46)).Take(15));
                WriteLog("CM server list loaded.");
                IsRunning = true;
                foreach (var bot in _bots)
                {
                    bot.Start();
                }
                _healthReportTimer.Start();
            }
            catch (CommunicationException e)
            {
                WriteLog(e.Message, EventLogEntryType.Warning);
            }
        }

        protected override void OnStop()
        {
            IsRunning = false;
            _healthReportTimer.Stop();

            if (Coodinator.State == CommunicationState.Faulted)
                Coodinator.Abort();
            else
                Coodinator.Close();

            WriteLog("Channel destroyed.");

            if (_bots != null)
            {
                foreach (var bot in _bots)
                {
                    bot.Dispose();
                }
                _bots = null;
                Thread.Sleep(1000);
            }
        }

        private async Task ReportBotHealthAsync()
        {
            await Coodinator.UpdateBotsAsync(_bots.Select(bot =>
            {
                var online = bot.State == Bot.BotState.LoggedOnOnline;
                var vm = new SteamBotVM
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

        private class SteamBotCoodinatorCallbackHandler : ISteamBotCoodinatorCallback
        {
            private readonly SteamBotService _botService;

            public SteamBotCoodinatorCallbackHandler(SteamBotService botService)
            {
                _botService = botService;
            }

            public void RemoveSteamFriend(string botId, string steamId)
            {
                var bot = _botService._bots.SingleOrDefault(b => b.Id == botId);
                if (bot != null && bot.State == Bot.BotState.LoggedOnOnline)
                    bot.RemoveFriend(steamId);
            }
        }
    }
}