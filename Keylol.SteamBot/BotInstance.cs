using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ChannelAdam.ServiceModel;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SteamKit2;
using SteamKit2.Internal;

namespace Keylol.SteamBot
{
    public class BotInstance : IDisposable
    {
        private readonly IServiceConsumer<ISteamBotCoordinator> _coordinator;
        private readonly ILog _logger;
        private readonly MqClientProvider _mqClientProvider;
        private readonly CallbackManager _callbackManager;
        private readonly SteamUser _steamUser;

        private bool _disposed;
        private bool _loginPending; // 是否处于登录过程中
        private bool _callbackPumpStarted; // 回调泵是否已启用
        private IModel _mqChannel;
        private static readonly Semaphore LoginSemaphore = new Semaphore(5, 5); // 最多 5 个机器人同时登录

        public string Id { get; set; }
        public int SequenceNumber { get; set; }
        public SteamUser.LogOnDetails LogOnDetails { get; set; } = new SteamUser.LogOnDetails();
        public SteamClient SteamClient { get; } = new SteamClient();
        public SteamFriends SteamFriends { get; }
        public BotCookieManager CookieManager { get; }

        public BotInstance(IServiceConsumer<ISteamBotCoordinator> coordinator, ILogProvider logProvider,
            BotCookieManager cookieManager, MqClientProvider mqClientProvider)
        {
            _coordinator = coordinator;
            _logger = logProvider.Logger;
            _mqClientProvider = mqClientProvider;
            CookieManager = cookieManager;

            _callbackManager = new CallbackManager(SteamClient);

            _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);
            _callbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKeyReceived);

            _callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaStateChanged);
            _callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendListUpdated);
            _callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessageReceived);

            _steamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
        }

        /// <summary>
        /// 启动机器人实例
        /// </summary>
        /// <param name="startWait">是否等待三秒后再启动，默认 <c>false</c></param>
        public void Start(bool startWait = false)
        {
            if (_disposed)
            {
                _logger.Fatal($"#{SequenceNumber} Try to restart disposed bot.");
                throw new InvalidOperationException("Try to restart disposed bot.");
            }

            if (!_callbackPumpStarted)
            {
                _callbackPumpStarted = true;
                Task.Run(() =>
                {
                    _logger.Info($"#{SequenceNumber} Listening callbacks...");
                    while (!_disposed)
                    {
                        _callbackManager.RunWaitCallbacks(TimeSpan.FromMilliseconds(10));
                    }
                    _logger.Info($"#{SequenceNumber} Stopped listening callbacks.");
                });
            }

            _coordinator.Consume(coordinator => coordinator.UpdateBot(Id, null, false, null));
            if (startWait)
            {
                _logger.Info($"#{SequenceNumber} Starting in 3 seconds...");
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }

            var sfhPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{LogOnDetails.Username}.sfh");
            if (File.Exists(sfhPath))
            {
                LogOnDetails.SentryFileHash = File.ReadAllBytes(sfhPath);
                _logger.Info($"#{SequenceNumber} Use sentry file hash from {LogOnDetails.Username}.sfh.");
            }

            if (!_loginPending)
            {
                LoginSemaphore.WaitOne();
                _loginPending = true;
            }
            SteamClient.Connect();

            _mqChannel = _mqClientProvider.CreateModel();
            _mqChannel.BasicQos(0, 5, false);
            var queueName = $"{MqClientProvider.SteamBotDelayedActionQueue}.{Id}";
            _mqChannel.QueueDeclare(queueName, true, false, false, null);
            _mqChannel.QueueBind(queueName, MqClientProvider.DelayedMessageExchange, queueName);
            var consumer = new EventingBasicConsumer(_mqChannel);
            consumer.Received += OnDelayedActionReceived;
            _mqChannel.BasicConsume(queueName, false, consumer);
        }

        /// <summary>
        /// 重启机器人实例
        /// </summary>
        public void Restart()
        {
            Stop();
            Start(true);
        }

        /// <summary>
        /// 停止机器人实例
        /// </summary>
        public void Stop()
        {
            if (_loginPending)
            {
                LoginSemaphore.Release();
                _loginPending = false;
            }
            _mqChannel?.Close();
            SteamClient.Disconnect();
            _logger.Info($"#{SequenceNumber} Stopped.");
        }

        private void OnDelayedActionReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                using (var streamReader = new StreamReader(new MemoryStream(basicDeliverEventArgs.Body)))
                {
                    var serializer = new JsonSerializer();
                    var actionDto =
                        serializer.Deserialize<SteamBotDelayedActionDto>(new JsonTextReader(streamReader));
                    switch (actionDto.Type)
                    {
                        case SteamBotDelayedActionType.SendChatMessage:
                        {
                            var steamId = new SteamID();
                            steamId.SetFromSteam3String((string) actionDto.Properties.SteamId);
                            var message = (string) actionDto.Properties.Message;
                            SteamFriends.SendChatMessage(steamId, EChatEntryType.ChatMsg, message);
                            try
                            {
                                if ((bool) actionDto.Properties.LogMessage)
                                {
                                    var friendName = SteamFriends.GetFriendPersonaName(steamId);
                                    _logger.Info($"#{SequenceNumber} [Chat TX] To {friendName} ({steamId}): {message}");
                                }
                            }
                            catch (RuntimeBinderException)
                            {
                            }
                            break;
                        }

                        case SteamBotDelayedActionType.RemoveFriend:
                        {
                            var steamId = new SteamID();
                            steamId.SetFromSteam3String((string) actionDto.Properties.SteamId);
                            var skip = SteamFriends.GetFriendRelationship(steamId) != EFriendRelationship.Friend;
                            try
                            {
                                if (!skip && (bool) actionDto.Properties.OnlyIfNotKeylolUser)
                                {
                                    var result = _coordinator.Consume(
                                        coordinator => coordinator.IsKeylolUser(steamId.Render(true)));
                                    if (!result.HasNoException || result.Value)
                                    {
                                        skip = true;
                                    }
                                }
                            }
                            catch (RuntimeBinderException)
                            {
                            }
                            if (!skip)
                            {
                                try
                                {
                                    SteamFriends.SendChatMessage(steamId, EChatEntryType.ChatMsg,
                                        (string) actionDto.Properties.Message);
                                }
                                catch (RuntimeBinderException)
                                {
                                }
                                SteamFriends.RemoveFriend(steamId);
                            }
                            break;
                        }

                        default:
                            _logger.Fatal($"#{SequenceNumber} Invalid delay action type: {actionDto.Type}.");
                            break;
                    }
                    _mqChannel.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
                }
            }
            catch (Exception e)
            {
                _mqChannel.BasicNack(basicDeliverEventArgs.DeliveryTag, false, false);
                _logger.Fatal($"#{SequenceNumber} Unhandled MQ consumer exception.", e);
            }
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
            _steamUser.LogOn(LogOnDetails);
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback disconnectedCallback)
        {
            if (disconnectedCallback.UserInitiated) return;
            _logger.Warn($"#{SequenceNumber} Disconnected.");
            Restart();
        }

        #endregion

        #region SteamUser Callbacks

        private async void OnLoggedOn(SteamUser.LoggedOnCallback loggedOnCallback)
        {
            switch (loggedOnCallback.Result)
            {
                case EResult.OK:
                    CookieManager.BotSequenceNumber = SequenceNumber;
                    CookieManager.ConnectedUniverse = SteamClient.ConnectedUniverse;
                    CookieManager.SteamId = _steamUser.SteamID;
                    CookieManager.WebApiUserNonce = loggedOnCallback.WebAPIUserNonce;
                    _logger.Info($"#{SequenceNumber} logged on.");
                    try
                    {
                        await SteamFriends.SetPersonaName($"其乐机器人 Keylol.com #{SequenceNumber}");
                        await SteamFriends.SetPersonaState(EPersonaState.LookingToPlay);
                        _coordinator.Consume(
                            coordinator => coordinator.UpdateBot(Id, null, true, _steamUser.SteamID.Render(true)));
                        _logger.Info($"#{SequenceNumber} is now online.");
                        var playGameMessage = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
                        playGameMessage.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                        {
                            game_id = new GameID(250820) // 默认玩 SteamVR
                        });
                        SteamClient.Send(playGameMessage);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.Fatal($"#{SequenceNumber} Set online timeout.");
                        Restart();
                        return;
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
            if (_loginPending)
            {
                LoginSemaphore.Release();
                _loginPending = false;
            }
        }

        private void OnUpdateMachineAuth(SteamUser.UpdateMachineAuthCallback updateMachineAuthCallback)
        {
            int fileSize;
            byte[] sentryHash;
            using (
                var fs =
                    File.Open(
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
            CookieManager.LoginKeyUniqueId = loginKeyCallback.UniqueID;
            CookieManager.CookiesExpired += async (sender, args) =>
            {
                try
                {
                    var webApiUserNonceCallback = await _steamUser.RequestWebAPIUserNonce();
                    if (webApiUserNonceCallback.Result != EResult.OK)
                        _logger.Warn(
                            $"#{SequenceNumber} Invalid WebAPIUserNonceCallback result: {webApiUserNonceCallback.Result}");
                    CookieManager.WebApiUserNonce = webApiUserNonceCallback.Nonce;
                    await CookieManager.Refresh();
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
            await CookieManager.Refresh();
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
            foreach (var friend in friendsListCallback.FriendList.Where(f => f.SteamID.IsIndividualAccount))
            {
                if (friendsListCallback.Incremental || friend.Relationship == EFriendRelationship.RequestRecipient ||
                    friend.Relationship == EFriendRelationship.None)
                {
                    var friendName = SteamFriends.GetFriendPersonaName(friend.SteamID);
                    _logger.Info(
                        $"#{SequenceNumber} Relationship with {friendName} ({friend.SteamID.Render(true)}) changed to {friend.Relationship}.");
                }
                switch (friend.Relationship)
                {
                    case EFriendRelationship.RequestRecipient:
                        _coordinator.Consume(
                            coordinator => coordinator.OnBotNewFriendRequest(friend.SteamID.Render(true), Id));
                        break;

                    case EFriendRelationship.None:
                        _coordinator.Consume(
                            coordinator => coordinator.OnUserBotRelationshipNone(friend.SteamID.Render(true), Id));
                        break;
                }
            }
            _coordinator.Consume(coordinator => coordinator.UpdateBot(Id, SteamFriends.GetFriendCount(), null, null));
        }

        private void OnFriendMessageReceived(SteamFriends.FriendMsgCallback friendMsgCallback)
        {
            if (friendMsgCallback.EntryType != EChatEntryType.ChatMsg) return;
            var friendName = SteamFriends.GetFriendPersonaName(friendMsgCallback.Sender);
            _logger.Info(
                $"#{SequenceNumber} [Chat RX] {friendName} ({friendMsgCallback.Sender.Render(true)}): {friendMsgCallback.Message}");
            _coordinator.Consume(
                coordinator =>
                    coordinator.OnBotNewChatMessage(friendMsgCallback.Sender.Render(true), Id, friendMsgCallback.Message));
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
                CookieManager.Dispose();
                Stop();
            }
            _disposed = true;
        }
    }
}