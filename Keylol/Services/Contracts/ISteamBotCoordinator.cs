using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DTO;

namespace Keylol.Services.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof (ISteamBotCoordinatorCallback))]
    public interface ISteamBotCoordinator
    {
        [OperationContract]
        Task<IEnumerable<SteamBotDto>> AllocateBots();

        [OperationContract(IsOneWay = true)]
        Task UpdateBots(IList<SteamBotUpdateRequestDto> vms);

        [OperationContract(IsOneWay = true)]
        Task SetUserStatus(string steamId, StatusClaim status);

        [OperationContract(IsOneWay = true)]
        Task SetUserSteamProfileName(string steamId, string name);

        [OperationContract(IsOneWay = true)]
        Task DeleteBindingToken(string botId, string steamId);

        [OperationContract]
        Task<string> GetCMServer();

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
    }

    public interface ISteamBotCoordinatorCallback
    {
        [OperationContract(IsOneWay = true)]
        void RemoveSteamFriend(string botId, string steamId);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string botId, string steamId, string message);

        [OperationContract]
        string FetchUrl(string botId, string url);
    }

    [DataContract]
    public enum StatusClaim
    {
        [EnumMember] Normal,
        [EnumMember] Probationer
    }
}