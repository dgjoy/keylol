using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
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

        private readonly Timer _healthReportTimer = new Timer(60*1000); // 60s
        private readonly object _consoleInputLock = new object();
        private readonly object _consoleOutputLock = new object();
        private bool _isRunning;

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
                WriteLog($"Application data location: {_appDataFolder}");
                _coodinator = CreateProxy();
                WriteLog("Channel created.");
                WriteLog($"Coodinator endpoint: {_coodinator.Endpoint.Address}");
//                var cmServer = await _coodinator.GetCMServerAsync();
//                var parts = cmServer.Split(':');
//                _cmServer = new IPEndPoint(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
//                WriteLog($"CM Server: {_cmServer}");
                await SteamDirectory.Initialize(148);
                WriteLog("CM server list loaded.");
                var bots = await _coodinator.AllocateBotsAsync();
                WriteLog($"{bots.Length} {(bots.Length > 1 ? "bots" : "bot")} allocated.");
                _isRunning = true;
                _bots = bots.Select(bot => new Bot(this, bot)).ToArray();
                _healthReportTimer.Start();
            }
            catch (Exception e)
            {
                WriteLog(e.Message, EventLogEntryType.Warning);
            }
        }

        protected override void OnStop()
        {
            _isRunning = false;
            _healthReportTimer.Stop();

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

            private const string SteamCommunityDomain = "steamcommunity.com";
            private const string SteamCommunityUrlBase = "http://steamcommunity.com/";

            private readonly SteamBotService _botService;
            private readonly SteamUser.LogOnDetails _logOnDetails = new SteamUser.LogOnDetails();
            private readonly SteamClient _steamClient = new SteamClient();
            private readonly CallbackManager _callbackManager;
            private readonly SteamUser _steamUser;
            private readonly SteamFriends _steamFriends;
            private BotState _state = BotState.Disconnected;
            private uint _loginKeyUniqueId;
            private string _webAPIUserNonce;
            private CookieContainer _cookies;
            private readonly Timer _cookiesCheckTimer = new Timer(10*60*1000); // 10min

            public string Id { get; }

            public BotState State
            {
                get { return _state; }
                private set
                {
                    if (_state != value)
                    {
                        WriteLog($"State changed: {_state} -> {value}.");
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
                var sfhPath = Path.Combine(_botService._appDataFolder, $"{_logOnDetails.Username}.sfh");
                if (File.Exists(sfhPath))
                {
                    _logOnDetails.SentryFileHash = File.ReadAllBytes(sfhPath);
                    WriteLog($"Use sentry file hash from {_logOnDetails.Username}.sfh.");
                }

                _steamUser = _steamClient.GetHandler<SteamUser>();
                _steamFriends = _steamClient.GetHandler<SteamFriends>();
                _callbackManager = new CallbackManager(_steamClient);

                _callbackManager.Subscribe(SafeCallback<SteamClient.ConnectedCallback>(OnConnected));
                _callbackManager.Subscribe(SafeCallback<SteamClient.DisconnectedCallback>(OnDisconnected));
//                _callbackManager.Subscribe<SteamClient.CMListCallback>(callback =>
//                {
//                    foreach (var s in callback.Servers)
//                    {
//                        WriteLog(s.ToString());
//                    }
//                });
                _callbackManager.Subscribe(SafeCallback<SteamUser.LoggedOnCallback>(OnLoggedOn));
                _callbackManager.Subscribe(SafeCallback<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth));
                _callbackManager.Subscribe(SafeCallback<SteamUser.LoginKeyCallback>(OnLoginKeyReceived));
                _callbackManager.Subscribe(SafeCallback<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged));
                _callbackManager.Subscribe(SafeCallback<SteamFriends.FriendsListCallback>(OnFriendListUpdated));
                _callbackManager.Subscribe(SafeCallback<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived));

                _cookiesCheckTimer.Elapsed += CookiesCheckTimerOnElapsed;

                Task.Run(() =>
                {
                    while (_botService._isRunning)
                    {
                        _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(100));
                    }
                    WriteLog("Callback pump stopped.");
                });

                _steamClient.Connect();
            }

            private Action<T> SafeCallback<T>(Action<T> action) where T : CallbackMsg
            {
                return callbackMsg =>
                {
                    if (_botService._isRunning)
                        action(callbackMsg);
                };
            }

            public void RemoveFriend(string steamId)
            {
                var id = new SteamID();
                id.SetFromSteam3String(steamId);
                _steamFriends.RemoveFriend(id);
            }

            private void WriteLog(string message, EventLogEntryType type = EventLogEntryType.Information)
            {
                _botService.WriteLog($"[{_logOnDetails.Username}] {message}", type);
            }

            #region SteamKit Callback

            #region SteamClient

            private void OnConnected(SteamClient.ConnectedCallback callback)
            {
                if (callback.Result == EResult.OK)
                {
                    State = BotState.ConnectedNotLoggedOn;
                    WriteLog("Connected.");
                    _steamUser.LogOn(_logOnDetails);
                }
            }

            private async void OnDisconnected(SteamClient.DisconnectedCallback callback)
            {
                State = BotState.Disconnected;
                if (!callback.UserInitiated)
                {
                    WriteLog("Disconnected. Try reconnecting...",
                        EventLogEntryType.Warning);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    _steamClient.Connect();
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
                        _webAPIUserNonce = callback.WebAPIUserNonce;
                        _steamFriends.SetPersonaName("其乐机器人 Keylol.comm");
                        _callbackManager.Subscribe(_steamFriends.SetPersonaState(EPersonaState.Online),
                            SafeCallback<SteamFriends.PersonaChangeCallback>(async personaChangeCallback =>
                            {
                                State = BotState.LoggedOnOnline;
                                WriteLog("Successfully logged on.", EventLogEntryType.SuccessAudit);
                                await _botService.ReportBotHealthAsync(this);
                            }));
                        break;

                    case EResult.AccountLogonDenied:
                    case EResult.InvalidLoginAuthCode:
                        State = BotState.MachineAuthPending;
                        WriteLog("Need auth code to log on.", EventLogEntryType.FailureAudit);
                        lock (_botService._consoleInputLock)
                        {
                            if (Environment.UserInteractive)
                            {
                                Console.WriteLine("Please input auth code for this bot:");
                                var authCode = Console.ReadLine();
                                _logOnDetails.AuthCode = authCode;
                            }
                        }
                        break;

                    default:
                        WriteLog($"Failed to log on: {callback.Result}.", EventLogEntryType.FailureAudit);
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

                File.WriteAllBytes(Path.Combine(_botService._appDataFolder, $"{_logOnDetails.Username}.sfh"), hash);
                // .sfh means Sentry File Hash
                _logOnDetails.SentryFileHash = hash;
                WriteLog($"Sentry file hash has been written to {_logOnDetails.Username}.sfh.");

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

            private async void OnLoginKeyReceived(SteamUser.LoginKeyCallback callback)
            {
                _loginKeyUniqueId = callback.UniqueID;
                await UpdateCookiesAsync();
                _cookiesCheckTimer.Start();
            }

            #endregion

            #region SteamFriend

            private async void OnPersonaStateChanged(SteamFriends.PersonaStateCallback callback)
            {
                if (!callback.FriendID.IsIndividualAccount) return;
                if (callback.FriendID == _steamUser.SteamID) return;
                await _botService._coodinator.SetUserSteamProfileNameAsync(callback.FriendID.Render(true),
                    callback.Name);
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
                        _steamFriends.RemoveFriend(steamId);
                        WriteLog($"Friend {steamId} has been removed. (Not Keylol user)");
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
                            WriteLog($"Friend {friend.SteamID} has unknown relationship {friend.Relationship}.",
                                EventLogEntryType.Warning);
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

            #region Cookies Check

            private async Task<HttpWebResponse> RequestAsync(string url, string method, NameValueCollection data = null,
                bool ajax = true, string referer = "")
            {
                // Append the data to the URL for GET-requests
                var isGetMethod = method.ToLower() == "get";
                var dataString = data == null
                    ? null
                    : string.Join("&", Array.ConvertAll(data.AllKeys,
                        key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(data[key])}"));

                if (isGetMethod && !string.IsNullOrEmpty(dataString))
                {
                    url += (url.Contains("?") ? "&" : "?") + dataString;
                }

                // Setup the request
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.Method = method;
                request.Accept = "application/json, text/javascript;q=0.9, */*;q=0.5";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                // request.Host is set automatically
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
                request.Referer = string.IsNullOrEmpty(referer) ? "http://steamcommunity.com/trade/1" : referer;
                request.Timeout = 50000; // Timeout after 50 seconds
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                if (ajax)
                {
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    request.Headers.Add("X-Prototype-Version", "1.7");
                }

                // Cookies
                request.CookieContainer = _cookies;

                // Write the data to the body for POST and other methods
                if (!isGetMethod && !string.IsNullOrEmpty(dataString))
                {
                    var dataBytes = Encoding.UTF8.GetBytes(dataString);
                    request.ContentLength = dataBytes.Length;

                    using (var requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                    }
                }

                // Get the response
                return await request.GetResponseAsync() as HttpWebResponse;
            }

            private async void CookiesCheckTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
            {
                using (var response = await RequestAsync(SteamCommunityUrlBase, "HEAD"))
                {
                    var cookieIsValid = response.Cookies["steamLogin"] == null ||
                                        !response.Cookies["steamLogin"].Value.Equals("deleted");
                    if (cookieIsValid) return;
                    WriteLog("Invalid cookies detected.", EventLogEntryType.Warning);
                    _callbackManager.Subscribe(_steamUser.RequestWebAPIUserNonce(),
                        SafeCallback<SteamUser.WebAPIUserNonceCallback>(async callback =>
                        {
                            if (callback.Result == EResult.OK)
                                _webAPIUserNonce = callback.Nonce;
                            await UpdateCookiesAsync();
                        }));
                }
            }

            private async Task UpdateCookiesAsync()
            {
                using (dynamic userAuth = WebAPI.GetAsyncInterface("ISteamUserAuth"))
                {
                    // generate an AES session key
                    var sessionKey = CryptoHelper.GenerateRandomBlock(32);

                    // RSA encrypt it with the public key for the universe we're on
                    byte[] encryptedSessionKey;
                    using (var rsa = new RSACrypto(KeyDictionary.GetPublicKey(_steamClient.ConnectedUniverse)))
                    {
                        encryptedSessionKey = rsa.Encrypt(sessionKey);
                    }

                    var loginKey = new byte[20];
                    Array.Copy(Encoding.ASCII.GetBytes(_webAPIUserNonce), loginKey, _webAPIUserNonce.Length);

                    // AES encrypt the loginkey with our session key
                    var encryptedLoginKey = CryptoHelper.SymmetricEncrypt(loginKey, sessionKey);

                    for (var i = 1; i <= 3; i++)
                    {
                        if (i == 1)
                            WriteLog("Try acquiring cookies...");
                        else
                            WriteLog($"Try acquiring cookies... [{i}]", EventLogEntryType.Warning);
                        try
                        {
                            KeyValue authResult =
                                await userAuth.AuthenticateUser(steamid: _steamClient.SteamID.ConvertToUInt64(),
                                    sessionkey: HttpUtility.UrlEncode(encryptedSessionKey),
                                    encrypted_loginkey: HttpUtility.UrlEncode(encryptedLoginKey),
                                    method: "POST",
                                    secure: true);

                            _cookies = new CookieContainer();

                            _cookies.Add(new Cookie("sessionid",
                                Convert.ToBase64String(Encoding.UTF8.GetBytes(_loginKeyUniqueId.ToString())),
                                string.Empty,
                                SteamCommunityDomain));

                            _cookies.Add(new Cookie("steamLogin", authResult["token"].AsString(),
                                string.Empty,
                                SteamCommunityDomain));

                            _cookies.Add(new Cookie("steamLoginSecure", authResult["tokensecure"].AsString(),
                                string.Empty,
                                SteamCommunityDomain));

                            await _botService._coodinator.UpdateCookiesAsync(Id,
                                _cookies.GetCookieHeader(new Uri(SteamCommunityUrlBase)));
                            WriteLog("Cookies acquired.", EventLogEntryType.SuccessAudit);
                            return;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    WriteLog("Failed to get cookies.", EventLogEntryType.FailureAudit);
                }
            }

            #endregion

            #region IDisposable Members and Helpers

            private bool _disposed;

            private void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    State = BotState.Disposing;
                    WriteLog("Disposing...");
                    _cookiesCheckTimer.Stop();
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