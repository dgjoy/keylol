using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DTO;

namespace Keylol.Services.Contracts
{
    /// <summary>
    ///     SteamBot 协作器
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ISteamBotCoordinatorCallback))]
    public interface ISteamBotCoordinator
    {
        /// <summary>
        ///     心跳测试
        /// </summary>
        [OperationContract]
        void Ping();

        /// <summary>
        ///     请求分配机器人，协作器在计算好机器人数量后将通过 RequestReallocateBots 回调通知客户端
        /// </summary>
        [OperationContract(IsOneWay = true)]
        Task RequestBots();

        /// <summary>
        ///     请求分配指定数量的机器人
        /// </summary>
        /// <param name="count">要求分配的数量</param>
        /// <returns>分配给客户端的机器人列表</returns>
        [OperationContract]
        List<SteamBotDto> AllocateBots(int count);

        /// <summary>
        ///     更新指定用户的属性
        /// </summary>
        /// <param name="steamId">要更新的用户 Steam ID</param>
        /// <param name="profileName">Steam 昵称，<c>null</c> 表示不更新</param>
        [OperationContract]
        Task UpdateUser(string steamId, string profileName);

        /// <summary>
        ///     更新指定机器人的属性
        /// </summary>
        /// <param name="id">要更新的机器人 ID</param>
        /// <param name="friendCount">好友数，<c>null</c> 表示不更新</param>
        /// <param name="online">是否在线，<c>null</c> 表示不更新</param>
        /// <param name="steamId">Steam ID，<c>null</c> 表示不更新</param>
        [OperationContract]
        Task UpdateBot(string id, int? friendCount = null, bool? online = null, string steamId = null);

        /// <summary>
        ///     判断指定 Steam 账户是不是其乐用户并且匹配指定机器人
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        /// <param name="botId">机器人 ID</param>
        /// <returns><c>true</c> 表示是其乐用户并于目标机器人匹配，<c>false</c> 表示不是</returns>
        [OperationContract]
        Task<bool> IsKeylolUser(string steamId, string botId);

        /// <summary>
        ///     当机器人接收到用户好友请求时，通过此方法通知协调器
        /// </summary>
        /// <param name="userSteamId">用户 Steam ID</param>
        /// <param name="botId">机器人 ID</param>
        [OperationContract(IsOneWay = true)]
        Task OnBotNewFriendRequest(string userSteamId, string botId);

        /// <summary>
        ///     当用户与机器人不再为好友时，通过此方法通知协调器
        /// </summary>
        /// <param name="userSteamId">用户 Steam ID</param>
        /// <param name="botId">机器人 ID</param>
        [OperationContract(IsOneWay = true)]
        Task OnUserBotRelationshipNone(string userSteamId, string botId);

        /// <summary>
        ///     当机器人收到新的聊天消息时，通过此方法通知协调器
        /// </summary>
        /// <param name="senderSteamId">消息发送人 Steam ID</param>
        /// <param name="botId">机器人 ID</param>
        /// <param name="message">聊天消息内容</param>
        [OperationContract(IsOneWay = true)]
        Task OnBotNewChatMessage(string senderSteamId, string botId, string message);
    }

    /// <summary>
    ///     SteamBot 协作器 客户端回调
    /// </summary>
    [ServiceContract]
    public interface ISteamBotCoordinatorCallback
    {
        /// <summary>
        ///     获取客户端已分配的机器人 ID 列表
        /// </summary>
        /// <returns>机器人 ID 列表，<c>null</c> 表示客户端还没有被分配过机器人</returns>
        [OperationContract]
        Task<List<string>> GetAllocatedBots();

        /// <summary>
        ///     要求重新为客户端分配指定数量的机器人
        /// </summary>
        /// <param name="count">机器人数量</param>
        /// <returns>如果目标分配数量小于当前已经分配的机器人数量，则返回被停止的机器人 ID 列表</returns>
        [OperationContract]
        Task<List<string>> RequestReallocateBots(int count);

        /// <summary>
        ///     要求客户端停止指定机器人实例
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        [OperationContract(IsOneWay = true)]
        Task StopBot(string botId);

        /// <summary>
        ///     要求机器人添加指定用户为好友
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="steamId">用户 Steam ID</param>
        [OperationContract(IsOneWay = true)]
        Task AddFriend(string botId, string steamId);

        /// <summary>
        ///     要求机器人删除指定好友
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="steamId">用户 Steam ID</param>
        [OperationContract(IsOneWay = true)]
        Task RemoveFriend(string botId, string steamId);

        /// <summary>
        ///     要求机器人为指定好友发送聊天消息
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="steamId">用户 Steam ID</param>
        /// <param name="message">聊天消息内容</param>
        /// <param name="logMessage">是否将消息记录到日志中，默认 false</param>
        [OperationContract(IsOneWay = true)]
        Task SendChatMessage(string botId, string steamId, string message, bool logMessage = false);

        /// <summary>
        ///     要求机器人向所有好友发送聊天消息
        /// </summary>
        /// <param name="message">聊天消息内容</param>
        [OperationContract(IsOneWay = true)]
        Task BroadcastMessage(string message);

        /// <summary>
        ///     获取指定用户的头像 Hash
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="steamId">用户 Steam ID</param>
        /// <returns>头像 Hash，<c>null</c> 表示获取失败</returns>
        [OperationContract]
        Task<string> GetUserAvatarHash(string botId, string steamId);

        /// <summary>
        ///     获取指定用户的 Steam 昵称
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="steamId">用户 Steam ID</param>
        /// <returns>Steam 昵称，<c>null</c> 表示获取失败</returns>
        [OperationContract]
        Task<string> GetUserProfileName(string botId, string steamId);

        /// <summary>
        ///     获取指定机器人的好友列表
        /// </summary>
        /// <returns>好友 Steam ID 列表，<c>null</c> 表示获取失败</returns>
        [OperationContract]
        Task<List<string>> GetFriendList(string botId);

        /// <summary>
        ///     设置机器人正在玩的游戏
        /// </summary>
        /// <param name="botId">机器人 ID，<c>null</c> 表示所有机器人</param>
        /// <param name="appId">App ID，0 表示停止游戏</param>
        [OperationContract(IsOneWay = true)]
        Task SetPlayingGame(string botId, int appId);

        /// <summary>
        ///     要求机器人使用自己的 Cookies 抓取指定网页（使用 HTTP GET 方法）
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        /// <param name="url">URL</param>
        /// <returns>响应的 HTTP Body，<c>null</c> 表示获取失败</returns>
        [OperationContract]
        Task<string> Curl(string botId, string url);
    }
}