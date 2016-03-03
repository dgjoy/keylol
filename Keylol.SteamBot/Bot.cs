using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Keylol.SteamBot.ServiceReference;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace Keylol.SteamBot
{
    public class Bot : IDisposable
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
        private uint _loginKeyUniqueId;
        private string _webApiUserNonce;
        private readonly Timer _cookiesCheckTimer = new Timer(10*60*1000); // 10min
        public readonly Crawler Crawler;

        public string Id { get; }

        public BotState State { get; private set; } = BotState.Disconnected;

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
            Crawler = new Crawler();
            _botService = botService;
            Id = botCredentials.Id;
            _logOnDetails.Username = botCredentials.SteamUserName;
            _logOnDetails.Password = botCredentials.SteamPassword;
            var sfhPath = Path.Combine(_botService.AppDataFolder, $"{_logOnDetails.Username}.sfh");
            if (File.Exists(sfhPath))
            {
                _logOnDetails.SentryFileHash = File.ReadAllBytes(sfhPath);
                WriteLog($"Use sentry file hash from {_logOnDetails.Username}.sfh.");
            }

            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();
            _callbackManager = new CallbackManager(_steamClient);

            _callbackManager.Subscribe(SafeCallback<SteamClient.ConnectedCallback>(OnConnected));
            _callbackManager.Subscribe(SafeCallbackAsync<SteamClient.DisconnectedCallback>(OnDisconnected));
            _callbackManager.Subscribe(SafeCallback<SteamUser.LoggedOnCallback>(OnLoggedOn));
            _callbackManager.Subscribe(SafeCallback<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth));
            _callbackManager.Subscribe(SafeCallbackAsync<SteamUser.LoginKeyCallback>(OnLoginKeyReceived));
            _callbackManager.Subscribe(SafeCallbackAsync<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged));
            _callbackManager.Subscribe(SafeCallbackAsync<SteamFriends.FriendsListCallback>(OnFriendListUpdated));
            _callbackManager.Subscribe(SafeCallbackAsync<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived));

            _cookiesCheckTimer.Elapsed += CookiesCheckTimerOnElapsed;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                while (_botService.IsRunning)
                {
                    try
                    {
                        _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(100));
                    }
                    catch (CommunicationException e)
                    {
                        WriteLog($"Communication exception occurred: {e.Message}", EventLogEntryType.Warning);
                        break;
                    }
                    catch (AggregateException e)
                    {
                        WriteLog($"Ignore unexpected exception: {e.InnerException.Message}", EventLogEntryType.Warning);
                    }
                    catch (Exception e)
                    {
                        WriteLog($"Ignore unexpected exception: {e.Message}", EventLogEntryType.Warning);
                    }
                }
                WriteLog("Callback pump stopped.");
            });

            _steamClient.Connect();
        }

        private Action<T> SafeCallback<T>(Action<T> action) where T : CallbackMsg
        {
            return callbackMsg =>
            {
                if (_botService.IsRunning)
                    action(callbackMsg);
            };
        }

        private Action<T> SafeCallbackAsync<T>(Func<T, Task> action) where T : CallbackMsg
        {
            return callbackMsg =>
            {
                if (_botService.IsRunning)
                    action(callbackMsg).Wait();
            };
        }

        public void SendMessage(string steamId, string message)
        {
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            _steamFriends.SendChatMessage(id, EChatEntryType.ChatMsg, message);
        }

        public void RemoveFriend(string steamId)
        {
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            _steamFriends.RemoveFriend(id);
        }

        private async Task ReportBotHealthAsync()
        {
            var online = State == BotState.LoggedOnOnline;
            var vm = new SteamBotVM
            {
                Id = Id,
                Online = online
            };
            if (online)
            {
                vm.FriendCount = FriendCount;
                vm.SteamId = SteamId;
            }
            await _botService.Coodinator.UpdateBotsAsync(new[] {vm});
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

        private async Task OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            State = BotState.Disconnected;
            if (!callback.UserInitiated)
            {
                _cookiesCheckTimer.Stop();
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
                    _webApiUserNonce = callback.WebAPIUserNonce;
                    _steamFriends.SetPersonaName("其乐机器人 Keylol.com");
                    _callbackManager.Subscribe(_steamFriends.SetPersonaState(EPersonaState.Online),
                        SafeCallbackAsync<SteamFriends.PersonaChangeCallback>(async personaChangeCallback =>
                        {
                            State = BotState.LoggedOnOnline;
                            WriteLog("Successfully logged on.", EventLogEntryType.SuccessAudit);
                            await ReportBotHealthAsync();
                        }));
                    break;

                case EResult.AccountLogonDenied:
                case EResult.InvalidLoginAuthCode:
                    State = BotState.MachineAuthPending;
                    WriteLog("Need auth code to log on.", EventLogEntryType.FailureAudit);
                    if (Environment.UserInteractive)
                    {
                        lock (SteamBotService.ConsoleInputLock)
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

            File.WriteAllBytes(Path.Combine(_botService.AppDataFolder, $"{_logOnDetails.Username}.sfh"), hash);
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

        private async Task OnLoginKeyReceived(SteamUser.LoginKeyCallback callback)
        {
            _loginKeyUniqueId = callback.UniqueID;
            await UpdateCookiesAsync();
            _cookiesCheckTimer.Start();
        }

        #endregion

        #region SteamFriend

        private async Task OnPersonaStateChanged(SteamFriends.PersonaStateCallback callback)
        {
            if (!callback.FriendID.IsIndividualAccount) return;
            if (callback.FriendID == _steamUser.SteamID) return;

            var steamId = callback.FriendID.Render(true);
            await _botService.Coodinator.SetUserSteamProfileNameAsync(steamId, callback.Name);
        }

        private async Task OnFriendListUpdated(SteamFriends.FriendsListCallback callback)
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
                        _botService.Coodinator.GetUsersBySteamIdsAsync(
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
                var friendSteamId = friend.SteamID.Render(true);
                switch (friend.Relationship)
                {
                    case EFriendRelationship.RequestRecipient:
                        user = await _botService.Coodinator.GetUserBySteamIdAsync(friendSteamId);
                        if (user == null)
                        {
                            _steamFriends.AddFriend(friend.SteamID);
                            WriteLog($"Accepted friend request from {friendSteamId}. (New user)");
                            _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                "欢迎使用当前 Steam 账号加入其乐，请输入您在网页上获取的 8 位绑定验证码。");
                            await _botService.Coodinator.BroadcastBotOnFriendAddedAsync(Id);
                            var timer = new Timer(300000) {AutoReset = false};
                            timer.Elapsed += (sender, args) =>
                            {
                                if (_steamFriends.GetFriendRelationship(friend.SteamID) ==
                                    EFriendRelationship.Friend &&
                                    _botService.Coodinator.GetUserBySteamId(friendSteamId) == null)
                                {
                                    _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                        "抱歉，您的会话因超时被强制结束，机器人已将您从好友列表中暂时移除。若要加入其乐，请重新按照网页指示注册账号。");
                                    _steamFriends.RemoveFriend(friend.SteamID);
                                    WriteLog($"Friend {friendSteamId} removed. (Operation timeout)");
                                }
                            };
                            timer.Start();
                        }
                        else
                        {
                            if (user.SteamBot.Id == Id)
                            {
                                _steamFriends.AddFriend(friend.SteamID);
                                WriteLog($"Accepted friend request from {friendSteamId}. (Rebinding)");
                                await _botService.Coodinator.SetUserStatusAsync(friendSteamId, StatusClaim.Normal);
                                _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                    "您已成功与其乐机器人再次绑定，请务必不要将其乐机器人从好友列表中移除。");
                            }
                            else
                            {
                                _steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg,
                                    "此 Steam 帐号已经与另外一位其乐机器人绑定，您即将被当前机器人从好友列表中移除。请通过口令组合登录并在设置中按提示重新添加机器人。");
                                _steamFriends.RemoveFriend(friend.SteamID);
                                WriteLog($"Rejected friend request from {friendSteamId}. (Already binded)");
                            }
                        }
                        break;

                    case EFriendRelationship.Friend:
                        break;

                    case EFriendRelationship.None:
                        user = await _botService.Coodinator.GetUserBySteamIdAsync(friendSteamId);
                        if (user == null)
                        {
                            await _botService.Coodinator.DeleteBindingTokenAsync(Id, friendSteamId);
                        }
                        else if (user.SteamBot.Id == Id)
                        {
                            WriteLog($"Friend {friendSteamId} removed this bot from his friend list.");
                            await _botService.Coodinator.SetUserStatusAsync(friendSteamId, StatusClaim.Probationer);
                        }
                        break;

                    default:
                        WriteLog($"Friend {friendSteamId} has unknown relationship {friend.Relationship}.",
                            EventLogEntryType.Warning);
                        break;
                }
            }
            await ReportBotHealthAsync();
        }

        private async Task OnFriendMessageReceived(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType != EChatEntryType.ChatMsg) return;

            var friendSteamId = callback.Sender.Render(true);
            var friendName = _steamFriends.GetFriendPersonaName(callback.Sender);
            WriteLog($"Message from {friendSteamId} ({friendName}): {callback.Message}");

            var user = await _botService.Coodinator.GetUserBySteamIdAsync(friendSteamId);
            if (user == null)
            {
                if (
                    await
                        _botService.Coodinator.BindSteamUserWithBindingTokenAsync(callback.Message.Trim(), Id,
                            friendSteamId,
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
                var match = Regex.Match(callback.Message, @"^\s*(\d{4})\s*$");
                if (match.Success)
                {
                    if (
                        await
                            _botService.Coodinator.BindSteamUserWithLoginTokenAsync(friendSteamId, match.Groups[1].Value))
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "欢迎回来，您已成功登录其乐社区。");
                    }
                    else
                    {
                        _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                            "您的输入无法被识别，请确认登录验证码的长度和格式。如果需要帮助，请与其乐职员取得联系。");
                    }
                }
                else
                {
                    var resultStream = await Utils.Retry(async () =>
                        await Crawler.HttpClient.GetStreamAsync(
                            $"http://www.tuling123.com/openapi/api?key={Crawler.TuringRobotApiKey}&info={HttpUtility.UrlEncode(callback.Message)}&userid={callback.Sender.ConvertToUInt64()}"));
                    if (resultStream == null) return;
                    using (var reader = new StreamReader(resultStream))
                    {
                        var result = JToken.ReadFrom(new JsonTextReader(reader));
                        switch ((int) result["code"])
                        {
                            case 100000: // 文字类
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    (string) result["text"]);
                                break;
                            }

                            case 200000: // 链接类
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    $"{(string) result["text"]}\n{(string) result["url"]}");
                                break;
                            }

                            case 302000: // 新闻
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    $"{(string) result["text"]}\n{string.Join("\n\n", result["list"].Select(news => $"{news["article"]}\n{news["detailurl"]}"))}");
                                break;
                            }

                            case 305000: // 列车
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    $"{(string) result["text"]}\n{string.Join("\n\n", result["list"].Select(train => $"{train["trainnum"]}\n{train["start"]} --> {train["terminal"]}\n{train["starttime"]} --> {train["endtime"]}"))}");
                                break;
                            }

                            case 306000: // 航班
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    $"{(string) result["text"]}\n{string.Join("\n\n", result["list"].Select(flight => $"{flight["flight"]}\n{flight["starttime"]} --> {flight["endtime"]}"))}");
                                break;
                            }

                            case 308000: // 菜谱
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    $"{(string) result["text"]}\n{string.Join("\n\n", result["list"].Select(dish => $"{dish["name"]}\n{dish["info"]}\n{dish["detailurl"]}"))}");
                                break;
                            }

                            default:
                            {
                                _steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,
                                    (string) result["text"]);
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Cookies Check

        private void CookiesCheckTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            using (var response =
                Utils.Retry(async () => await Crawler.RequestAsync(Crawler.SteamCommunityUrlBase, "HEAD")).Result)
            {
                var cookieIsValid = response.Cookies["steamLogin"] == null ||
                                    !response.Cookies["steamLogin"].Value.Equals("deleted");
                response.Close();
                if (cookieIsValid) return;
                WriteLog("Invalid cookies detected.", EventLogEntryType.Warning);
                _callbackManager.Subscribe(_steamUser.RequestWebAPIUserNonce(),
                    SafeCallbackAsync<SteamUser.WebAPIUserNonceCallback>(async callback =>
                    {
                        if (callback.Result == EResult.OK)
                            _webApiUserNonce = callback.Nonce;
                        await UpdateCookiesAsync();
                    }));
            }
        }

        private async Task UpdateCookiesAsync()
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
            Array.Copy(Encoding.ASCII.GetBytes(_webApiUserNonce), loginKey, _webApiUserNonce.Length);

            // AES encrypt the loginkey with our session key
            var encryptedLoginKey = CryptoHelper.SymmetricEncrypt(loginKey, sessionKey);

            var success = await Utils.Retry(async () =>
            {
                using (dynamic userAuth = WebAPI.GetAsyncInterface("ISteamUserAuth"))
                {
                    KeyValue authResult =
                        await userAuth.AuthenticateUser(steamid: _steamClient.SteamID.ConvertToUInt64(),
                            sessionkey: HttpUtility.UrlEncode(encryptedSessionKey),
                            encrypted_loginkey: HttpUtility.UrlEncode(encryptedLoginKey),
                            method: "POST",
                            secure: true);

                    Crawler.Cookies.Add(new Cookie("sessionid",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(_loginKeyUniqueId.ToString())),
                        string.Empty,
                        Crawler.SteamCommunityDomain));

                    Crawler.Cookies.Add(new Cookie("steamLogin", authResult["token"].AsString(),
                        string.Empty,
                        Crawler.SteamCommunityDomain));

                    Crawler.Cookies.Add(new Cookie("steamLoginSecure", authResult["tokensecure"].AsString(),
                        string.Empty,
                        Crawler.SteamCommunityDomain));

                    WriteLog("Cookies acquired.", EventLogEntryType.SuccessAudit);
                }
            }, i =>
            {
                if (i == 1)
                    WriteLog("Try acquiring cookies...");
                else
                    WriteLog($"Try acquiring cookies... [{i}]", EventLogEntryType.Warning);
            });

            if (!success)
                WriteLog("Failed to get cookies.", EventLogEntryType.FailureAudit);
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