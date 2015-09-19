using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Keylol.SteamBot.ServiceReference;
using SteamKit2;
using Timer = System.Timers.Timer;

namespace Keylol.SteamBot
{
    public partial class SteamBotService : ServiceBase
    {
        private readonly SteamBotCoodinatorClient _coodinator;
        private Bot[] _bots;

        private readonly string _appDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Keylol Steam Bot Service");

        private readonly Timer _healthReportTimer = new Timer(60000) {AutoReset = false};
        private readonly object _consoleInputLock = new object();
        private readonly object _consoleOutputLock = new object();

        public SteamBotService()
        {
            InitializeComponent();
            _coodinator = new SteamBotCoodinatorClient(new InstanceContext(new SteamBotCoodinatorCallbackHandler(this)));
            if (_coodinator.ClientCredentials != null)
            {
                _coodinator.ClientCredentials.UserName.UserName = "keylol-bot";
                _coodinator.ClientCredentials.UserName.Password = "neLFDyJB8Vj2Xtsn2KMTUEFw";
            }
            _healthReportTimer.Elapsed += async (sender, args) => await ReportBotHealthAsync();
        }

        private void WriteLog(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            if (Environment.UserInteractive)
            {
                lock (_consoleOutputLock)
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

        protected override void OnStart(string[] args)
        {
            Directory.CreateDirectory(_appDataFolder);
            Task.Run(async () =>
            {
                var bots = await _coodinator.AllocateBotsAsync();
                WriteLog($"{bots.Length} {(bots.Length > 1 ? "bots" : "bot")} allocated.");
                _bots = bots.Select(bot => new Bot(this, bot)).ToArray();
                _healthReportTimer.Start();
            });
        }

        protected override void OnStop()
        {
            _healthReportTimer.Stop();
            _coodinator.Close();
            foreach (var bot in _bots)
            {
                bot.Dispose();
            }
        }

        private async Task ReportBotHealthAsync()
        {
            await _coodinator.UpdateBotsAsync(_bots.Select(bot =>
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
            if (_healthReportTimer.Enabled)
                _healthReportTimer.Stop();
            _healthReportTimer.Start();
        }

        public void TestStartupAndStop(string[] args)
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
            Thread.Sleep(5000);
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

        private class Bot : IDisposable
        {
            public enum BotState
            {
                Disconnected,
                ConnectedNotLoggedOn,
                MachineAuthPending,
                LoggedOnNotOnline,
                LoggedOnOnline,
                Disposing
            }

            private readonly SteamBotService _botService;
            private readonly SteamUser.LogOnDetails _logOnDetails = new SteamUser.LogOnDetails();
            private readonly SteamClient _steamClient = new SteamClient();
            private readonly CallbackManager _callbackManager;
            private readonly SteamUser _steamUser;
            private readonly SteamFriends _steamFriends;
            private bool _isRunning = true;
            private BotState _state = BotState.Disconnected;

            public string Id { get; }

            public BotState State
            {
                get { return _state; }
                private set
                {
                    if (_state != value)
                    {
                        _botService.WriteLog($"Bot {Id} state changed: {_state} -> {value}.");
                        _state = value;
                    }
                }
            }

            public string SteamId => _steamUser.SteamID.ToString();

            public int FriendCount => _steamFriends.GetFriendCount();

            public Bot(SteamBotService botService, SteamBotDTO botCredentials)
            {
                _botService = botService;
                Id = botCredentials.Id;
                _logOnDetails.Username = botCredentials.SteamUserName;
                _logOnDetails.Password = botCredentials.SteamPassword;
                var sfhPath = Path.Combine(_botService._appDataFolder, $"{Id}.sfh");
                if (File.Exists(sfhPath))
                {
                    _logOnDetails.SentryFileHash = File.ReadAllBytes(sfhPath);
                    _botService.WriteLog($"Use sentry file hash from {Id}.sfh.");
                }

                _steamUser = _steamClient.GetHandler<SteamUser>();
                _steamFriends = _steamClient.GetHandler<SteamFriends>();
                _callbackManager = new CallbackManager(_steamClient);

                Task.Run(() =>
                {
                    while (_isRunning)
                    {
                        _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(500));
                    }
                    _botService.WriteLog($"Bot {Id} callback pump stopped.");
                });

                _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                _callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);
                _callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged);
                _callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendListUpdated);
                _callbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);
                _callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived);

                _steamClient.Connect();
            }

            public void RemoveFriend(string steamId)
            {
                _steamFriends.RemoveFriend(new SteamID(steamId));
            }

            #region SteamKit Callback

            #region SteamClient

            private void OnConnected(SteamClient.ConnectedCallback callback)
            {
                if (callback.Result == EResult.OK)
                {
                    State = BotState.ConnectedNotLoggedOn;
                    _botService.WriteLog($"Bot {Id} connected.");
                    _steamUser.LogOn(_logOnDetails);
                }
            }

            private async void OnDisconnected(SteamClient.DisconnectedCallback callback)
            {
                State = BotState.Disconnected;
                if (!callback.UserInitiated)
                {
                    _botService.WriteLog($"Bot {Id} disconnected. Try reconnecting...",
                        EventLogEntryType.Warning);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    _steamClient.Connect();
                }
                else
                {
                    _botService.WriteLog($"Bot {Id} disconnected.");
                    _isRunning = false;
                }
            }

            #endregion

            #region SteamUser

            private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
            {
                switch (callback.Result)
                {
                    case EResult.OK:
                        State = BotState.LoggedOnNotOnline;
                        _steamFriends.SetPersonaName("其乐机器人");
                        _steamFriends.SetPersonaState(EPersonaState.Online);
                        break;

                    case EResult.AccountLogonDenied:
                    case EResult.InvalidLoginAuthCode:
                        State = BotState.MachineAuthPending;
                        _botService.WriteLog($"Bot {Id} need auth code to log on.",
                            EventLogEntryType.FailureAudit);
                        lock (_botService._consoleInputLock)
                        {
                            if (Environment.UserInteractive)
                            {
                                Console.WriteLine($"Please input auth code for bot {Id}:");
                                var authCode = Console.ReadLine();
                                _logOnDetails.AuthCode = authCode;
                            }
                        }
                        break;

                    default:
                        _botService.WriteLog($"Bot {Id} failed to log on: {callback.Result}.",
                            EventLogEntryType.FailureAudit);
                        break;
                }
            }

            private void OnUpdateMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
            {
                var sha1Managed = new SHA1Managed();
                var hash = sha1Managed.ComputeHash(callback.Data);
                sha1Managed.Clear();

                File.WriteAllBytes(Path.Combine(_botService._appDataFolder, $"{Id}.sfh"), hash);
                // .sfh means Sentry File Hash
                _logOnDetails.SentryFileHash = hash;
                _botService.WriteLog($"Sentry file hash has been written to {Id}.sfh.");

                var authResponse = new SteamUser.MachineAuthDetails
                {
                    BytesWritten = callback.BytesToWrite,
                    FileName = callback.FileName,
                    FileSize = callback.BytesToWrite,
                    Offset = callback.Offset,
                    SentryFileHash = hash, // Should be the sha1 hash of the sentry file we just wrote

                    OneTimePassword = callback.OneTimePassword,
                    // Not sure on this one yet, since we've had no examples of steam using OTPs

                    LastError = 0, // Result from win32 GetLastError
                    Result = EResult.OK, // If everything went okay, otherwise ~who knows~
                    JobID = callback.JobID, // So we respond to the correct server job
                };

                _steamUser.SendMachineAuthResponse(authResponse);
            }

            #endregion

            #region SteamFriend

            private void OnPersonaStateChanged(SteamFriends.PersonaStateCallback callback)
            {
                if (callback.FriendID == _steamUser.SteamID && callback.State == EPersonaState.Online)
                {
                    State = BotState.LoggedOnOnline;
                    _botService.WriteLog($"Bot {Id} successfully logged on.", EventLogEntryType.SuccessAudit);
                }
            }

            private void OnFriendListUpdated(SteamFriends.FriendsListCallback callback)
            {
                //TODO
            }

            private async void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
            {
                var user = await _botService._coodinator.GetUserBySteamIdAsync(callback.SteamID.ToString());
                if (user == null)
                {
                    _steamFriends.AddFriend(callback.SteamID);
                }
                else
                {
                    if (user.SteamBot.Id == Id)
                    {
                        _botService.WriteLog(
                            $"Bot {Id} alert: an already binded user {user.SteamId} add the same bot as friend again.");
                    }
                    else
                    {
                        _steamFriends.RemoveFriend(callback.SteamID);
                    }
                }
            }

            private async void OnFriendMessageReceived(SteamFriends.FriendMsgCallback callback)
            {
                if (callback.EntryType != EChatEntryType.ChatMsg) return;

                var user = await _botService._coodinator.GetUserBySteamIdAsync(callback.Sender.ToString());
                if (user == null)
                {
                    if (await _botService._coodinator.BindSteamUserWithBindingTokenAsync(
                        callback.Sender.ToString(), callback.Message, Id))
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "绑定成功");
                    }
                    else
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "验证码无效");
//                        _steamFriends.RemoveFriend(callback.Sender);
                    }
                }
                else
                {
                    if (await _botService._coodinator.BindSteamUserWithLoginTokenAsync(
                        callback.Sender.ToString(), callback.Message))
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "登录成功");
                    }
                    else
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "验证码无效");
                    }
                }
            }

            #endregion

            #endregion

            #region IDisposable Members and Helpers

            private bool _disposed;

            private void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    State = BotState.Disposing;
                    _botService.WriteLog($"Bot {Id} is disposing...");
                    _steamClient.Disconnect();
                }
                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~Bot()
            {
                Dispose(false);
            }

            #endregion
        }
    }
}