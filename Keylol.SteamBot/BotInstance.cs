using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ChannelAdam.ServiceModel;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;
using SteamKit2;
using SteamKit2.Internal;

namespace Keylol.SteamBot
{
    public class BotInstance : IDisposable
    {
        private readonly IServiceConsumer<ISteamBotCoordinator> _coordinator;
        private readonly ILog _logger;
        private readonly BotCookieManager _cookieManager;
        private readonly CallbackManager _callbackManager;
        private readonly SteamClient _steamClient = new SteamClient();
        private readonly SteamUser _steamUser;
        private readonly SteamFriends _steamFriends;

        private bool _disposed;
        private static readonly Semaphore LoginSemaphore = new Semaphore(5, 5); // 最多 5 个机器人同时登录

        public string Id { get; set; }

        public int SequenceNumber { get; set; }

        public SteamUser.LogOnDetails LogOnDetails { get; set; } = new SteamUser.LogOnDetails();

        public BotInstance(IServiceConsumer<ISteamBotCoordinator> coordinator, ILogProvider logProvider,
            BotCookieManager cookieManager)
        {
            _coordinator = coordinator;
            _logger = logProvider.Logger;
            _cookieManager = cookieManager;

            _callbackManager = new CallbackManager(_steamClient);

            _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);
            _callbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKeyReceived);

            _callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged);
            _callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendListUpdated);
            _callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived);

            _steamUser = _steamClient.GetHandler<SteamUser>();
            _steamFriends = _steamClient.GetHandler<SteamFriends>();
        }

        /// <summary>
        /// 启动机器人实例
        /// </summary>
        public void Start()
        {
            var sfhPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{LogOnDetails.Username}.sfh");
            if (File.Exists(sfhPath))
            {
                LogOnDetails.SentryFileHash = File.ReadAllBytes(sfhPath);
                _logger.Info($"#{SequenceNumber} Use sentry file hash from {LogOnDetails.Username}.sfh.");
            }
            Task.Run(() =>
            {
                _logger.Info($"#{SequenceNumber} Bot started.");
                while (!_disposed)
                {
                    _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(100));
                }
                _logger.Info($"#{SequenceNumber} Bot stopped.");
            });

            _steamClient.Connect();
        }

        /// <summary>
        /// 重启机器人实例（断开重连 Steam）
        /// </summary>
        public void Restart()
        {
            _steamClient.Disconnect();
            _steamClient.Connect();
        }

        #region SteamClient Callbacks

        private void OnConnected(SteamClient.ConnectedCallback connectedCallback)
        {
            if (connectedCallback.Result != EResult.OK)
            {
                _logger.Fatal($"#{SequenceNumber} Connected callback invalid result: {connectedCallback.Result}");
                return;
            }
            _logger.Info($"#{SequenceNumber} Connected.");
            LoginSemaphore.WaitOne(TimeSpan.FromSeconds(30));
            _steamUser.LogOn(LogOnDetails);
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback disconnectedCallback)
        {
            if (disconnectedCallback.UserInitiated) return;
            _logger.Warn($"#{SequenceNumber} Disconnected, retrying in 3 seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _steamClient.Connect();
        }

        #endregion

        #region SteamUser Callbacks

        private async void OnLoggedOn(SteamUser.LoggedOnCallback loggedOnCallback)
        {
            switch (loggedOnCallback.Result)
            {
                case EResult.OK:
                    _cookieManager.BotSequenceNumber = SequenceNumber;
                    _cookieManager.ConnectedUniverse = _steamClient.ConnectedUniverse;
                    _cookieManager.SteamId = _steamUser.SteamID;
                    _cookieManager.WebApiUserNonce = loggedOnCallback.WebAPIUserNonce;
                    _logger.Info($"#{SequenceNumber} logged on.");
                    try
                    {
                        await _steamFriends.SetPersonaName($"其乐机器人 Keylol.com #{SequenceNumber}");
                        await _steamFriends.SetPersonaState(EPersonaState.LookingToPlay);
                        _coordinator.Consume(
                            coordinator => coordinator.UpdateBot(Id, null, true, _steamUser.SteamID.Render(true)));
                        _logger.Info($"#{SequenceNumber} is now online.");
                        var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
                        playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                        {
                            game_id = new GameID(250820) // SteamVR
                        });
                        _steamClient.Send(playGame);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.Fatal($"#{SequenceNumber} Set online timeout.");
                        Restart();
                    }
                    break;

                case EResult.AccountLogonDenied:
                    _logger.Fatal(
                        $"#{SequenceNumber} Need auth code (from email {loggedOnCallback.EmailDomain}) to login.");
                    break;

                case EResult.AccountLoginDeniedNeedTwoFactor:
                    _logger.Fatal($"#{SequenceNumber} Need two-factor auth code (from authenticator app) to login.");
                    break;

                default:
                    _logger.Fatal($"#{SequenceNumber} LoggedOnCallback invalid result: {loggedOnCallback.Result}.");
                    break;
            }
            LoginSemaphore.Release();
        }

        private void OnUpdateMachineAuth(SteamUser.UpdateMachineAuthCallback updateMachineAuthCallback)
        {
            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{LogOnDetails.Username}.sfh"),
                FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(updateMachineAuthCallback.Offset, SeekOrigin.Begin);
                fs.Write(updateMachineAuthCallback.Data, 0, updateMachineAuthCallback.BytesToWrite);
                fileSize = (int) fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            LogOnDetails.SentryFileHash = sentryHash;
            _logger.Info($"#{SequenceNumber} Sentry file hash saved to {LogOnDetails.Username}.sfh.");

            _steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = updateMachineAuthCallback.JobID,
                FileName = updateMachineAuthCallback.FileName,
                BytesWritten = updateMachineAuthCallback.BytesToWrite,
                FileSize = fileSize,
                Offset = updateMachineAuthCallback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = updateMachineAuthCallback.OneTimePassword,
                SentryFileHash = sentryHash
            });
        }

        private async void OnLoginKeyReceived(SteamUser.LoginKeyCallback loginKeyCallback)
        {
            _cookieManager.LoginKeyUniqueId = loginKeyCallback.UniqueID;
            _cookieManager.CookiesExpired += async (sender, args) =>
            {
                try
                {
                    var webApiUserNonceCallback = await _steamUser.RequestWebAPIUserNonce();
                    if (webApiUserNonceCallback.Result != EResult.OK)
                        _logger.Warn(
                            $"#{SequenceNumber} Invalid WebAPIUserNonceCallback result: {webApiUserNonceCallback.Result}");
                    _cookieManager.WebApiUserNonce = webApiUserNonceCallback.Nonce;
                    await _cookieManager.Refresh();
                }
                catch (TaskCanceledException)
                {
                    _logger.Warn($"#{SequenceNumber} Request new WebAPIUserNonce timeout.");
                }
                catch (Exception e)
                {
                    _logger.Fatal($"#{SequenceNumber} Request new WebAPIUserNonce unhandled exception.", e);
                }
            };
            await _cookieManager.Refresh();
        }

        #endregion

        #region SteamFriend Callbacks

        private void OnPersonaStateChanged(SteamFriends.PersonaStateCallback personaStateCallback)
        {
            if (!personaStateCallback.FriendID.IsIndividualAccount) return;
            if (personaStateCallback.FriendID == _steamUser.SteamID) return;

            var steamId = personaStateCallback.FriendID.Render(true);
            _coordinator.Consume(coordinator => coordinator.UpdateUser(steamId, personaStateCallback.Name));
        }

        private void OnFriendListUpdated(SteamFriends.FriendsListCallback friendsListCallback)
        {
            _logger.Info($"#{SequenceNumber} {friendsListCallback}");
        }

        private void OnFriendMessageReceived(SteamFriends.FriendMsgCallback friendMsgCallback)
        {
            _logger.Info($"#{SequenceNumber} {friendMsgCallback}");
        }

        #endregion

        /// <summary>
        /// 停止机器人实例，并通知协调器撤销分配
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _cookieManager.Dispose();
                _coordinator.Operations.DeallocateBot(Id);
                _steamClient.Disconnect();
            }
            _disposed = true;
        }
    }
}