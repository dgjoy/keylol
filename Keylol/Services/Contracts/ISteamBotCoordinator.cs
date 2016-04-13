using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DTO;

namespace Keylol.Services.Contracts
{
    /// <summary>
    /// SteamBot 协作器
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof (ISteamBotCoordinatorCallback))]
    public interface ISteamBotCoordinator
    {
        /// <summary>
        /// 请求分配机器人，协作器在计算好机器人数量后将通过 RequestReallocateBots 回调通知客户端
        /// </summary>
        [OperationContract(IsOneWay = true)]
        Task RequestBots();

        /// <summary>
        /// 请求分配指定数量的机器人
        /// </summary>
        /// <param name="count">要求分配的数量</param>
        /// <returns>分配给客户端的机器人列表</returns>
        [OperationContract]
        Task<List<SteamBotDto>> AllocateBots(int count);

        /// <summary>
        /// 撤销对指定机器人的会话分配
        /// </summary>
        /// <param name="botId">机器人 ID</param>
        [OperationContract]
        Task DeallocateBot(string botId);

        /// <summary>
        /// 更新指定用户的属性
        /// </summary>
        /// <param name="steamId">要更新的用户 Steam ID</param>
        /// <param name="profileName">Steam 昵称，<c>null</c> 表示不更新</param>
        [OperationContract]
        Task UpdateUser(string steamId, string profileName);

        /// <summary>
        /// 更新指定机器人的属性
        /// </summary>
        /// <param name="id">要更新的机器人 ID</param>
        /// <param name="friendCount">好友数，<c>null</c> 表示不更新</param>
        /// <param name="online">是否在线，<c>null</c> 表示不更新</param>
        /// <param name="steamId">Steam ID，<c>null</c> 表示不更新</param>
        [OperationContract]
        Task UpdateBot(string id, int? friendCount = null, bool? online = null, string steamId = null);

        #region Deprecated

        [OperationContract(IsOneWay = true)]
        Task SetUserStatus(string steamId, StatusClaim status);

        [OperationContract(IsOneWay = true)]
        Task DeleteBindingToken(string botId, string steamId);

        [OperationContract]
        Task<UserDto> GetUserBySteamId(string steamId);

        [OperationContract]
        Task<IList<UserDto>> GetUsersBySteamIds(IList<string> steamIds);

        [OperationContract]
        Task<bool> BindSteamUserWithBindingToken(string code, string botId, string userSteamId,
            string userSteamProfileName, string userSteamAvatarHash);

        [OperationContract]
        Task<bool> BindSteamUserWithLoginToken(string userSteamId, string code);

        [OperationContract(IsOneWay = true)]
        Task BroadcastBotOnFriendAdded(string botId);

        #endregion
    }

    /// <summary>
    /// SteamBot 协作器 客户端回调
    /// </summary>
    [ServiceContract]
    public interface ISteamBotCoordinatorCallback
    {
        /// <summary>
        /// 获取客户端已分配的机器人 ID 列表
        /// </summary>
        /// <returns>机器人 ID 列表，<c>null</c> 表示客户端还没有被分配过机器人</returns>
        [OperationContract]
        Task<List<string>> GetAllocatedBots();

        /// <summary>
        /// 要求重新为客户端分配指定数量的机器人
        /// </summary>
        /// <param name="count">机器人数量</param>
        [OperationContract]
        Task RequestReallocateBots(int count);

        #region Deprecated

        [OperationContract(IsOneWay = true)]
        void RemoveSteamFriend(string botId, string steamId);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string botId, string steamId, string message);

        [OperationContract]
        string FetchUrl(string botId, string url);

        #endregion
    }

    [DataContract]
    public enum StatusClaim
    {
        [EnumMember] Normal,
        [EnumMember] Probationer
    }
}