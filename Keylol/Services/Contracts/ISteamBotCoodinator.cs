using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;

namespace Keylol.Services.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof (ISteamBotCoodinatorCallback))]
    public interface ISteamBotCoodinator
    {
        [OperationContract]
        Task<IEnumerable<SteamBotDTO>> AllocateBots();

        [OperationContract(IsOneWay = true)]
        Task UpdateBots(IList<SteamBotVM> vms);

        [OperationContract(IsOneWay = true)]
        Task SetUserStatus(string steamId, Contracts.StatusClaim status);

        [OperationContract(IsOneWay = true)]
        Task SetUserSteamProfileName(string steamId, string name);

        [OperationContract(IsOneWay = true)]
        Task DeleteBindingToken(string botId, string steamId);

        [OperationContract]
        Task<string> GetCMServer();

        [OperationContract]
        Task<UserDTO> GetUserBySteamId(string steamId);

        [OperationContract]
        Task<IList<UserDTO>> GetUsersBySteamIds(IList<string> steamIds);

        [OperationContract]
        Task<bool> BindSteamUserWithBindingToken(string code, string botId, string userSteamId,
            string userSteamProfileName, string userSteamAvatarHash);

        [OperationContract]
        Task<bool> BindSteamUserWithLoginToken(string userSteamId, string code);

        [OperationContract(IsOneWay = true)]
        Task BroadcastBotOnFriendAdded(string botId);
    }
    
    public interface ISteamBotCoodinatorCallback
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
        [EnumMember]
        Normal,
        [EnumMember]
        Probationer
    }
}