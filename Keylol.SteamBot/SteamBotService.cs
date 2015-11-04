using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Keylol.SteamBot.ServiceReference;
using SteamKit2;
using Timer = System.Timers.Timer;

namespace Keylol.SteamBot
{
    public partial class SteamBotService : ServiceBase
    {
        private SteamBotCoodinatorClient _coodinator;
        private Bot[] _bots;

        private readonly string _appDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Keylol Steam Bot Service");

        private readonly Timer _healthReportTimer = new Timer(60000) {AutoReset = false};
        private IPEndPoint _cmServer;
        private readonly object _consoleInputLock = new object();
        private readonly object _consoleOutputLock = new object();

        public SteamBotService()
        {
            InitializeComponent();
            _healthReportTimer.Elapsed += async (sender, args) => await ReportBotHealthAsync();
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

        protected override async void OnStart(string[] args)
        {
            try
            {
                Directory.CreateDirectory(_appDataFolder);
                _coodinator = CreateProxy();
                WriteLog("Channel created.");
                WriteLog($"Coodinator endpoint: {_coodinator.Endpoint.Address}");
                var cmServer = await _coodinator.GetCMServerAsync();
                var parts = cmServer.Split(':');
                _cmServer = new IPEndPoint(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
                WriteLog($"CM Server: {_cmServer}");
                var bots = await _coodinator.AllocateBotsAsync();
                WriteLog($"{bots.Length} {(bots.Length > 1 ? "bots" : "bot")} allocated.");
                _bots = bots.Select(bot => new Bot(this, bot)).ToArray();
                _healthReportTimer.Start();
            }
            catch (CommunicationException)
            {
            }
        }

        protected override void OnStop()
        {
            if (_healthReportTimer.Enabled)
            {
                _healthReportTimer.Stop();
            }

            if (_coodinator.State == CommunicationState.Faulted)
                _coodinator.Abort();
            else
                _coodinator.Close();

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

        private async Task ReportBotHealthAsync(Bot bot)
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
            await _coodinator.UpdateBotsAsync(new[] {vm});
        }

        public void ConsoleStartup(string[] args)
        {
            Console.WriteLine("Running in console mode. Press Ctrl-M to stop.");
            while (true)
            {
                try
                {
                    OnStart(args);
                    while (true)
                    {
                        var key = Console.ReadKey();
                        if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.M)
                            break;
                    }
                    OnStop();
                }
                catch (Exception)
                {
                    Console.WriteLine("Fatal error! Try restarting service...");
                    // ignored, auto restart service
                }
            }
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

            public string SteamId => _steamUser.SteamID.Render(true);

            public int FriendCount
            {
                get
                {
                    var total = _steamFriends.GetFriendCount();
                    var count = 0;
                    for (var i = 0; i < total; i++)
                    {
                        if (_steamFriends.GetFriendByIndex(i).IsIndividualAccount)
                            count++;
                    }
                    return count;
                }
            }

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

                _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
//                _callbackManager.Subscribe<SteamClient.CMListCallback>(callback =>
//                {
//                    foreach (var s in callback.Servers)
//                    {
//                        _botService.WriteLog(s.ToString());
//                    }
//                });
                _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                _callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);
                _callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged);
                _callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendListUpdated);
                _callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived);

                Task.Run(() =>
                {
                    while (_isRunning)
                    {
                        _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(500));
                    }
                    _botService.WriteLog($"Bot {Id} callback pump stopped.");
                });

                _steamClient.Connect(_botService._cmServer);
            }

            public void RemoveFriend(string steamId)
            {
                var id = new SteamID();
                id.SetFromSteam3String(steamId);
                _steamFriends.RemoveFriend(id);
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
                    _steamClient.Connect(_botService._cmServer);
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
                        _steamFriends.SetPersonaName("其乐机器人 Keylol.com");
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
                byte[] hash;
                using (var sha1 = SHA1.Create())
                {
                    hash = sha1.ComputeHash(callback.Data);
                }

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

            private async void OnPersonaStateChanged(SteamFriends.PersonaStateCallback callback)
            {
                if (!callback.FriendID.IsIndividualAccount) return;
                if (callback.FriendID == _steamUser.SteamID)
                {
                    if (callback.State != EPersonaState.Online) return;
                    State = BotState.LoggedOnOnline;
                    _botService.WriteLog($"Bot {Id} successfully logged on.", EventLogEntryType.SuccessAudit);
                    await _botService.ReportBotHealthAsync(this);
                }
                else
                {
                    await _botService._coodinator.SetUserSteamProfileNameAsync(callback.FriendID.Render(true),
                        callback.Name);
                }
            }

            private async void OnFriendListUpdated(SteamFriends.FriendsListCallback callback)
            {
                if (!callback.Incremental)
                {
                    var friends =
                        callback.FriendList.Where(
                            friend =>
                                friend.SteamID.IsIndividualAccount && friend.Relationship == EFriendRelationship.Friend)
                            .ToList();
                    var users =
                        await
                            _botService._coodinator.GetUsersBySteamIdsAsync(
                                friends.Select(friend => friend.SteamID.Render(true)).ToArray());
                    var friendsToRemove = friends.Select(friend => friend.SteamID)
                        .Except(users.Select(user =>
                        {
                            var steamId = new SteamID();
                            steamId.SetFromSteam3String(user.SteamId);
                            return steamId;
                        })).Concat(users.Where(user => user.SteamBot.Id != Id).Select(user =>
                        {
                            var steamId = new SteamID();
                            steamId.SetFromSteam3String(user.SteamId);
                            return steamId;
                        }));
                    foreach (var steamId in friendsToRemove)
                    {
                        _botService.WriteLog($"Friend {steamId} should be removed. (Not Keylol user)");
//                        _steamFriends.RemoveFriend(steamId);
                    }
                }
                foreach (var friend in callback.FriendList)
                {
                    if (!friend.SteamID.IsIndividualAccount)
                        continue;

                    UserDTO user;
                    switch (friend.Relationship)
                    {
                        case EFriendRelationship.RequestRecipient:
                            user = await _botService._coodinator.GetUserBySteamIdAsync(friend.SteamID.Render(true));
                            if (user == null)
                            {
                                _steamFriends.AddFriend(friend.SteamID);
                                _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                    "欢迎使用当前 Steam 账号加入其乐，请输入您在网页上获取的 8 位绑定验证码。");
                                await _botService._coodinator.BroadcastBotOnFriendAddedAsync(Id);
                                var timer = new Timer(300000) {AutoReset = false};
                                timer.Elapsed += async (sender, args) =>
                                {
                                    if (_steamFriends.GetFriendRelationship(friend.SteamID) ==
                                        EFriendRelationship.Friend &&
                                        (await
                                            _botService._coodinator.GetUserBySteamIdAsync(friend.SteamID.Render(true))) ==
                                        null)
                                    {
                                        _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                            "抱歉，您的会话因超时被强制结束，机器人已将您从好友列表中暂时移除。若要加入其乐，请重新按照网页指示注册账号。");
                                        _steamFriends.RemoveFriend(friend.SteamID);
                                    }
                                };
                                timer.Start();
                            }
                            else
                            {
                                if (user.SteamBot.Id == Id)
                                {
                                    _steamFriends.AddFriend(friend.SteamID);
                                    await
                                        _botService._coodinator.SetUserStatusAsync(friend.SteamID.Render(true),
                                            StatusClaim.Normal);
                                    _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                        "您已成功与其乐机器人再次绑定，请务必不要将其乐机器人从好友列表中移除。");
                                }
                                else
                                {
                                    _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                        "此 Steam 帐号已经与另外一位其乐机器人绑定，您即将被当前机器人从好友列表中移除。请通过口令组合登录并在设置中按提示重新添加机器人。");
                                    _steamFriends.RemoveFriend(friend.SteamID);
                                }
                            }
                            break;

                        case EFriendRelationship.Friend:
                            break;

                        case EFriendRelationship.None:
                            user = await _botService._coodinator.GetUserBySteamIdAsync(friend.SteamID.Render(true));
                            if (user == null)
                            {
                                await _botService._coodinator.DeleteBindingTokenAsync(Id, friend.SteamID.Render(true));
                            }
                            else if (user.SteamBot.Id == Id)
                            {
                                await _botService._coodinator.SetUserStatusAsync(friend.SteamID.Render(true),
                                    StatusClaim.Probationer);
                            }
                            break;

                        default:
                            _botService.WriteLog(
                                $"Friend {friend.SteamID} should be removed. (Unknown relationship {friend.Relationship})");
//                            _steamFriends.RemoveFriend(friend.SteamID);
                            break;
                    }
                }
                await _botService.ReportBotHealthAsync(this);
            }

            private async void OnFriendMessageReceived(SteamFriends.FriendMsgCallback callback)
            {
                if (callback.EntryType != EChatEntryType.ChatMsg) return;

                var user = await _botService._coodinator.GetUserBySteamIdAsync(callback.Sender.Render(true));
                if (user == null)
                {
                    if (await _botService._coodinator.BindSteamUserWithBindingTokenAsync(
                        callback.Message, Id, callback.Sender.Render(true),
                        _steamFriends.GetFriendPersonaName(callback.Sender),
                        BitConverter.ToString(_steamFriends.GetFriendAvatar(callback.Sender))
                            .Replace("-", string.Empty)
                            .ToLower()
                        ))
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                            "绑定成功，欢迎加入其乐！今后您可以向机器人发送对话快速登录社区，请勿将机器人从好友列表移除。");
                        var timer = new Timer(3000) {AutoReset = false};
                        timer.Elapsed += (sender, args) =>
                        {
                            _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                "若希望在其乐上获得符合游戏兴趣的据点推荐，请避免将 Steam 资料隐私设置为「仅自己可见」。");
                        };
                        timer.Start();
                    }
                    else
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                            "您的输入无法被识别，请确认绑定验证码的长度和格式。如果需要帮助，请与其乐职员取得联系。");
                    }
                }
                else
                {
                    if (await _botService._coodinator.BindSteamUserWithLoginTokenAsync(
                        callback.Sender.Render(true), callback.Message))
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "欢迎回来，您已成功登录其乐社区。");
                    }
                    else
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                            "您的输入无法被识别，请确认登录验证码的长度和格式。如果需要帮助，请与其乐职员取得联系。");
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
                    _isRunning = false;
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